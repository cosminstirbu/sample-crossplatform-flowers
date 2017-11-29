using System.Collections.Generic;
using System.Threading.Tasks;
using Flowers.Api.Data;

namespace Flowers.Model
{
    public interface IFlowersService
    {
        Task<IList<Flower>> Refresh();

        Task<bool> Save(Flower flower);
    }
}