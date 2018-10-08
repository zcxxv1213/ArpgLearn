using Assets.Scripts.Com.Game.Events;
using System.ComponentModel;

namespace ETModel
{
	public abstract class Object: EventDispatcherInterface,ISupportInitialize
    {
		public virtual void BeginInit()
		{
		}

		public virtual void EndInit()
		{
		}

		public override string ToString()
		{
            return this.ToString();
			//return JsonHelper.ToJson(this);
		}
	}
}