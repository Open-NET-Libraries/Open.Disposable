using System.Threading.Tasks;

namespace Open.Disposable.Async
{
	public interface IAsyncDisposable
	{
		ValueTask DisposeAsync();
	}
}
