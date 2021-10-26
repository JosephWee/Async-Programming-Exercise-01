using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MathWebApi.Controllers
{
    public class MathController : ApiController
    {
        public IHttpActionResult PostSum([FromBody] SumRequestModel requestModel)
        {
            try
            {
                int result = requestModel.operand.Length > 0 ? requestModel.operand[0] : 0;

                for (int i = 1; i < requestModel.operand.Length; i++)
                {
                    result += requestModel.operand[i];
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }
    }
}
