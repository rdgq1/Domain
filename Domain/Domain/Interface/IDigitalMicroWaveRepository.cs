using Domain.Model;
using System;
using System.Collections.Generic;

namespace Domain.Interface
{
    public interface IDigitalMicroWaveRepository
    {
        IList<JobTemplate> LoadDefaultTemplates();
        void SaveTemplatesToFile(String fullpath, IList<JobTemplate> templates);
        IList<JobTemplate> ReadTemplatesFromFile(String fullpath);
        String GetCurrentPath(String fileName);
        Boolean TryGetFullPath(String path, out String result);
        String ReadTextFile(String fullpath);
    }
}