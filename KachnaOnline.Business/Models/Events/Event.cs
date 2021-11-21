// Event.cs
// Author: David Chocholatý

using System;
using System.Collections.Generic;

namespace KachnaOnline.Business.Models.Events
{
    /// <summary>
    /// A model representing an event.
    /// </summary>
    public class Event : NewEvent
    {
        /// <summary>
        /// An ID of the event.
        /// </summary>
        public int Id { get; set; }
    }
}
