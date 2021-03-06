using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Weapons;
using Character;

namespace Equipments
{
    public class EquipmentMgr : MonoBehaviour
    {
        [SerializeField] GameObject _owner;
        public GameObject Owner => this._owner;

        [SerializeField] GameObject _objectWeapon;
        [SerializeField] GameObject _objectUtility;

        [SerializeField] GameObject _reserveWeapon;

        bool _isUsingWeapon;

        Equipment _weapon;
        public Equipment Weapons => this._weapon;

        BaseUtility _utility;
        public BaseUtility Utility => this._utility;

        void Awake()
        {
            this._weapon = this._objectWeapon.GetComponent<AmmoWeapon>();
            this._weapon.Owner = this._owner;
            this._weapon.OnUnequiped();

            if (this._objectUtility != null)
            {
                this._utility = this._objectUtility.GetComponent<BaseUtility>();
                this._utility.Owner = this._owner;
                this._utility.OnUnequiped();
                EventCenter.Publish(EventId.UTILITY_CHARACTER_OWN, this._owner);
            }

            this.EquipWeapon();
            this.SubEvents();
        }

        void Start()
        {
            this.EquipWeapon();
        }

        public void EquipUtility(bool hideWeapon = true)
        {
            if (!this.HasUtility) return;

            if (hideWeapon) this._weapon.OnUnequiped();
            this._utility.OnEquiped();
            this._isUsingWeapon = hideWeapon;

            EventCenter.Publish(EventId.UTILITY_CHARACTER_OWN, this._owner);
        }

        public void EquipWeapon()
        {
            if (this._weapon == null) return;
            this._isUsingWeapon = true;
            if (this.HasUtility) this._utility.OnUnequiped();
            this._weapon.OnEquiped();
        }

        public void EquipLasted()
        {
            if (this._isUsingWeapon) this.EquipWeapon();
            else this.EquipUtility();
        }

        public void UnequipAll()
        {
            if (this.HasUtility) this._utility.OnUnequiped();
            this._weapon.OnUnequiped();
        }

        public void SetWeaponToReserved()
        {
            if (this._reserveWeapon == null) return;

            this.UnequipAll();
            MeleeWeapon meleeWeapon = this._reserveWeapon.GetComponent<MeleeWeapon>();
            AmmoWeapon ammoWeapon = this._reserveWeapon.GetComponent<AmmoWeapon>();

            this._weapon = null;
            if (meleeWeapon != null) this._weapon = meleeWeapon;
            else if (ammoWeapon != null) this._weapon = ammoWeapon;

            if (this._weapon == null) return;
            this._weapon.Owner = this._owner;
            this._weapon.OnUnequiped();
            this.EquipWeapon();
        }

        void SubEvents()
        {
            this.SubEventCat();
            this.SubEventInteract();
        }

        void SubEventCat()
        {
            CharacterStats stats = this._owner.GetComponent<CharacterStats>();
            if (stats != null && stats.CharacterSide == CharacterSide.CATS)
            {
                EventCenter.Subcribe(EventId.CAT_DOWN, (object pubData) =>
                {
                    PubData.CatDown data = pubData as PubData.CatDown;
                    if (data.Dispatcher == this._owner) this.UnequipAll();
                });

                EventCenter.Subcribe(EventId.CAT_RECOVERED, (object pubData) =>
                {
                    GameObject dispatcher = pubData as GameObject;
                    if (dispatcher == this._owner) this.EquipLasted();
                });

                EventCenter.Subcribe(EventId.CAT_DYING, (object pubData) =>
                {
                    GameObject dispatcher = pubData as GameObject;
                    if (dispatcher == this._owner) this.SetWeaponToReserved();
                });
            }
        }

        void SubEventInteract()
        {
            EventCenter.Subcribe(EventId.INTERACT_START, (object pubData) =>
            {
                PubData.IneractStart data = pubData as PubData.IneractStart;
                if (data.Dispatcher == this.Owner) this.UnequipAll();
            });

            EventCenter.Subcribe(EventId.INTERACT_END, (object pubData) =>
            {
                PubData.InteractEnd data = pubData as PubData.InteractEnd;
                if (data.Dispatcher == this.Owner) this.EquipLasted();
            });
        }

        bool HasUtility => (this._objectUtility != null && this._utility != null);
    }
}