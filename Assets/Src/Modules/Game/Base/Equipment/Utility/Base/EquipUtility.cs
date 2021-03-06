using System.Collections;
using System.Collections.Generic;
using Equipments;
using UnityEngine;

namespace Utilities
{
    public class EquipUtility : BaseUtility
    {
        protected virtual bool Equip => !this.IsEquiped && InputMgr.StartUseUtil(this._owner);
        protected virtual bool Unequip => this.IsEquiped && InputMgr.StartUseUtil(this._owner);
        protected virtual bool Active => InputMgr.StartShoot(this._owner);

        protected override void Update()
        {
            //if object is equiped and has command active => Active
            if (this.IsEquiped)
            {
                if (this.Active && this.IsReady)
                {
                    this.ActiveUtil();
                }
                else if (this.Unequip) this.UnequipUtil();
            }
            //if object is not equiped and has command equip => Equip
            else
            {
                if (this.Equip) this.EquipUtil();
            }

            base.Update();
        }

        protected override void ActiveUtil()
        {
            base.ActiveUtil();
            this.UnequipUtil();
        }
    }
}
