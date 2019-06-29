/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIThttps://github.com/electricessence/Open.Disposable/blob/master/LISCENSE.md
 */
 
using System.Threading.Tasks;

namespace Open.Disposable.Async
{
	public interface IAsyncDisposable
	{
		ValueTask DisposeAsync();
	}
}
