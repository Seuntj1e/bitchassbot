using BitchAssBot.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BitchAssBot.Models
{
    public class CommandAction
    {
        public ActionType Type { get; set; }
        public int Units { get; set; }
        public Guid Id { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}