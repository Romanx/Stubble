﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stubble.Core.Classes.Tokens
{
    public class EscapedValueToken : ParserOutput, IRenderableToken
    {
        public string Render(Context context, IDictionary<string, string> partials, string originalTemplate)
        {
            throw new NotImplementedException();
        }
    }
}
