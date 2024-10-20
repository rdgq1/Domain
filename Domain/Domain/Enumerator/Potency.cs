using System.Runtime.Serialization;
sing System.Runtime.Serialization;

namespace Domain.Enumerator
{
    /// <summary>
    /// MicroWave potency
    /// </summary>
    public enum Potency
    {
        [EnumMember(Value = "One")]
        One = 1,
        [EnumMember(Value = "Two")]
        Two = 2,
        [EnumMember(Value = "Three")]
        Three = 3,
        [EnumMember(Value = "Four")]
        Four = 4,
        [EnumMember(Value = "Five")]
        Five = 5,
        [EnumMember(Value = "Six")]
        Six = 6,
        [EnumMember(Value = "Seven")]
        Seven = 7,
        [EnumMember(Value = "Eight")]
        Eight = 8,
        [EnumMember(Value = "Nine")]
        Nine = 9,
        [EnumMember(Value = "Ten")]
        Ten = 10
    }
}