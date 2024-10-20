using Domain.Enumerator;
using System;

namespace Domain.Domain.Base
{
    public abstract class BaseJob
    {
        public Potency Potency { get; protected set; }

        /// <summary>
        /// Time left to cook, in seconds
        /// </summary>
        public short TimeLeft { get; protected set; }
    }
}