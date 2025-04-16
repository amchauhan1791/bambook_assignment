using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nop.Web.API.Dtos;
public class AuthRequestModel
{
    [JsonProperty(propertyName: "username")]
    public string UserName { get; set; }

    [JsonProperty(propertyName: "password")]
    public string Password { get; set; }
}
