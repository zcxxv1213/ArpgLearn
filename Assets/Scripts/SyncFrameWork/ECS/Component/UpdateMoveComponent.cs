using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
    [ObjectSystem]
    public class UpdateMoveComponentUpdateSystem : UpdateSystem<UpdateMoveComponent>
    {
        public override void Update(UpdateMoveComponent self)
        {
            self.Update();
        }
    }
    public class UpdateMoveComponent : Component
    {
        public void Update()
        {
            Unit unit = this.GetParent<Unit>();
            unit.UpdatePosition();
        }
    }
}
