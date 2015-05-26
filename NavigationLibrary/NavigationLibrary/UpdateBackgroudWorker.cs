using System;
using System.Threading;
namespace NavigationLibrary
{
	public class UpdateBackgroudWorker
	{
		private enum UpdateBackgroundWorkerStates
		{
			Stopped,
			Started
		}
		public delegate void ThreadUpdateHandler(object sender);
		private readonly int _threadTimeoutMs = 10000;
		private bool _shouldStop;
		private AutoResetEvent _threadStartEvent;
		private AutoResetEvent _threadEndEvent;
		private Thread _markersPositionUpdateThread;
		private UpdateBackgroudWorker.UpdateBackgroundWorkerStates _state;
		public event UpdateBackgroudWorker.ThreadUpdateHandler ThreadUpdated;
		private void ThreadStartFunction()
		{
			this._threadStartEvent.Set();
			while (!this._shouldStop)
			{
				if (this.ThreadUpdated != null)
				{
					this.ThreadUpdated(this);
				}
			}
			if (this._threadEndEvent != null)
			{
				this._threadEndEvent.Set();
			}
		}
		public bool Start()
		{
			bool result;
			if (this._state == UpdateBackgroudWorker.UpdateBackgroundWorkerStates.Started)
			{
				result = true;
			}
			else if (!this._threadEndEvent.Reset())
			{
				result = false;
			}
			else if (!this._threadStartEvent.Reset())
			{
				result = false;
			}
			else
			{
				this._shouldStop = false;
				this._markersPositionUpdateThread = new Thread(new ThreadStart(this.ThreadStartFunction));
				this._markersPositionUpdateThread.Start();
				bool flag = this._threadStartEvent.WaitOne(this._threadTimeoutMs);
				if (flag)
				{
					this._state = UpdateBackgroudWorker.UpdateBackgroundWorkerStates.Started;
				}
				result = flag;
			}
			return result;
		}
		public bool Stop()
		{
			bool result;
			if (this._state == UpdateBackgroudWorker.UpdateBackgroundWorkerStates.Stopped)
			{
				result = true;
			}
			else
			{
				this._state = UpdateBackgroudWorker.UpdateBackgroundWorkerStates.Stopped;
				if (!this._threadEndEvent.Reset())
				{
					result = false;
				}
				else
				{
					this._shouldStop = true;
					result = this._threadEndEvent.WaitOne(this._threadTimeoutMs);
				}
			}
			return result;
		}
		public UpdateBackgroudWorker()
		{
			this._shouldStop = false;
			this._state = UpdateBackgroudWorker.UpdateBackgroundWorkerStates.Stopped;
			this._threadEndEvent = new AutoResetEvent(false);
			this._threadStartEvent = new AutoResetEvent(false);
		}
	}
}
