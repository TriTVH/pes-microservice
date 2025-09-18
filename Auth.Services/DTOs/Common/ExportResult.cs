using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTOs.Common
{
    public record ExportResult(byte[] FileContents, string FileName);
}
