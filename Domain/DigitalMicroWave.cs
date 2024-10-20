using Domain.Enumerator;
using Domain.Interface;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;

namespace Service
{
    public class DigitalMicroWaveService : IDigitalMicroWaveService
    {
        private DigitalMicroWave _microwave;
        private IDigitalMicroWaveRepository _repo;
        private const String _templateFileName = "JobTemplates.json";
        private const Potency _defaultPotency = Potency.Eight;
        private const Int32 _defaultTime = 30;
        private AutoResetEvent _autoEvent;
        private Timer _timer;

        ~DigitalMicroWaveService()
        {
            _repo = null;
            _timer.Dispose();
            _autoEvent.Dispose();
        }

        public DigitalMicroWaveService(IDigitalMicroWaveRepository repo)
        {
            _repo = repo;
        }

        public DigitalMicroWave Initialize()
        {
            try
            {
                var fullpath = _repo.GetCurrentPath(_templateFileName);

                var savedTemplates = _repo.ReadTemplatesFromFile(fullpath);
                this._microwave = new DigitalMicroWave(savedTemplates, _defaultTime, _defaultPotency);

                _autoEvent = new AutoResetEvent(false);

                if (_timer == null)
                    _timer = new Timer(this._microwave.Tick, _autoEvent, 1000, 1000);

                return this._microwave;
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao tentar inicializar o microondas:{ex.Message}", ex);
            }
        }

        public IList<JobTemplate> GetTemplateByNameKind(String name, MealKind? kind)
        {
            return _microwave.GetTemplateByNameKind(name, kind);
        }

        public IList<JobTemplate> SaveTemplate(JobTemplate newTemplate)
        {
            if (String.IsNullOrEmpty(newTemplate.Name?.Trim())) throw new Exception("Please give a name for the new template.");

            _microwave.SaveTemplate(newTemplate);
            return _microwave.GetTemplateByNameKind(String.Empty, newTemplate.MealKind);
        }

        public IList<JobTemplate> DeleteTemplate(JobTemplate template, MealKind? mealkind)
        {
            if (!template.CanDelete) throw new Exception("Cannot delete a pre-made template.");

            _microwave.DeleteTemplate(template);
            return _microwave.GetTemplateByNameKind(String.Empty, mealkind);
        }

        public void Start(String inputString)
        {
            if (this._microwave.Status == MicroWaveStatus.Ready || this._microwave.Status == MicroWaveStatus.JobLess)
            {
                try
                {
                    String absolutePath = String.Empty;
                    if (_repo.TryGetFullPath(inputString, out absolutePath))
                    {
                        String templateJson = _repo.ReadTextFile(absolutePath);
                        StartFromInputString(templateJson);
                    }
                    else
                    {
                        StartFromInputString(inputString);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Falha ao tentar estartar job:{ex.Message}");
                }
            }
            else
                throw new Exception("Microwave is already running a job.");
        }

        private void ValidatePotency(Int32 potency)
        {
            if (potency == 0) throw new Exception("Por favor informe uma potência entre Um e Dez..");
            if (potency < 1) throw new Exception("A potência mínima é Um.");
            if (potency > 10) throw new Exception("A potência máxima é Dez.");
        }

        private void ValidateTimeleft(Int32 timeLeft)
        {
            if (timeLeft == 0) throw new Exception("Por favor informe um tempo entre um segundo e dois minutos.");
            if (timeLeft > 120) throw new Exception("O tempo de aquecimento não pode ser superior a dois minutos.");
            if (timeLeft < 1) throw new Exception("O tempo de aquecimento não pode ser inferior a um segundo.");
        }

        private void ValidateInput(JobTemplate template)
        {
            if (template == null)
                throw new Exception("Por favor informe um aquecimento válido.");

            ValidateTimeleft(template.TimeLeft);
            ValidatePotency((Int32)template.Potency);
        }

        private void StartFromInputString(String inputString)
        {
            var template = GetJobFromInputString(inputString);
            ValidateInput(template);

            this._microwave.SetJobTemplate(template);
            this._microwave.StartJob();
        }

        private JobTemplate GetJobFromInputString(String inputString)
        {
            try
            {
                JobTemplateParameterLess template = JsonSerializer.Deserialize<JobTemplateParameterLess>(inputString);
                return template.Get();
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao tentar deserializar Input string:{ex.Message}", ex);
            }
        }

        public String SerializeCurrentJobTemplateToJson()
        {
            _microwave.ResetJob();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(_microwave.CurrentJob.Template, options);
        }

        public void ResetPotency(Decimal potency)
        {
            if (this._microwave.Status == MicroWaveStatus.Running || this._microwave.Status == MicroWaveStatus.DoorOpen)
                throw new Exception("A potência não pode ser alterada durante o aquecimento.");

            ValidatePotency((Int32)potency);

            this._microwave.OverridePotency((Potency)potency);
        }

        public void ResetTimeleft(DateTime timeleft)
        {
            if (this._microwave.Status == MicroWaveStatus.Running || this._microwave.Status == MicroWaveStatus.DoorOpen)
                throw new Exception("O tempo restante não pode ser alterado durante o aquecimento.");

            Int16 seconds = (Int16)(timeleft.Second + 60.0 * timeleft.Minute);
            ValidateTimeleft(seconds);

            this._microwave.OverrideTimeleft(seconds);
        }

        public void SetJobTemplate(JobTemplate template)
        {
            this._microwave.SetJobTemplate(template);
        }

        public MicroWaveStatus GetStatus()
        {
            return this._microwave.Status;
        }

        public String Cancel()
        {
            this._microwave.CancelJob();
            this._microwave.OverridePotency(_defaultPotency);
            this._microwave.OverrideTimeleft(_defaultTime);
            return this.SerializeCurrentJobTemplateToJson();
        }

        public DigitalMicroWave GetMicroWave()
        {
            return this._microwave;
        }

        public void Pause()
        {
            this._microwave.PauseJob();
        }

        public void Resume()
        {
            this._microwave.ResumeJob();
        }

        public void PersistTemplates()
        {
            try
            {
                var fullpath = _repo.GetCurrentPath(_templateFileName);
                IList<JobTemplate> templates = this._microwave.GetTemplateByNameKind(String.Empty, null);

                _repo.SaveTemplatesToFile(fullpath, templates);
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao  inicializar o microondas:{ex.Message}", ex);
            }
        }
    }
}
