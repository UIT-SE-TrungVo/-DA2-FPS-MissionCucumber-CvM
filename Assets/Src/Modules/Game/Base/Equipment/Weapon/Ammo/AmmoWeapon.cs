using System;
using System.Collections;
using System.Collections.Generic;
using Character;
using Equipments;
using UnityEngine;

namespace Weapons
{
    public class AmmoWeapon : Equipment, IWeapon
    {
        protected readonly Vector2 AIM_POINT = new Vector2(
            Screen.width * 0.5f,
            Screen.height * 0.5f
        );

        protected AmmoWeaponModel _model;
        public AmmoWeaponModel Model => this._model;

        protected int _isFiring;

        protected GunState _gunState;

        [SerializeField] protected Camera _eye;
        [SerializeField] protected AudioClip _soundEquip;
        [SerializeField] protected AudioClip _soundReload;

        protected override void Start()
        {
            base.Start();
            this.GetModel();
            this.Model.TotalAmmo -= this.Model.MagazineSize;
        }

        protected virtual void GetModel()
        {
            this._model = this.GetComponent<AmmoWeaponStats>().Model;
        }

        protected virtual void Update()
        {
            if (this.IsReady && this.CanShoot && InputMgr.StartShoot(this._owner))
            {
                this.Shoot();
            }
            else if (InputMgr.Reload(this._owner))
            {
                this.DoReload();
            }
        }

        protected virtual void Shoot()
        {
            this.Model.RemainAmmo--;

            var targets = this.Target;
            if (targets.Length > 0)
            {
                foreach (GameObject target in targets)
                {
                    Debug.Log(target);
                    this.DoHitEffect(target);
                }
            }

            this.PublishAmmoChange();
            this.RecalulateAmmo();
        }

        public override void OnEquiped()
        {
            base.OnEquiped();
            LeanTween.delayedCall(Mathf.Max(this._equipTime, Time.fixedDeltaTime), () =>
            {
                if (this != null)
                {
                    this.gameObject.PlaySound(_soundEquip);
                    this._gunState = GunState.READY_TO_FIRE;
                    EventCenter.Publish(
                        EventId.WEAPON_AMMO_EQUIP,
                        new PubData.WeaponAmmoEquip(this.Owner, this.Model.RemainAmmo, this.Model.TotalAmmo)
                    );
                    Crosshair.Ins.Show();
                }
            });
        }

        public override void OnUnequiped()
        {
            base.OnUnequiped();
            EventCenter.Publish(
                EventId.WEAPON_UNEQUIP,
                new PubData.WeaponUnequip(this.Owner)
            );
        }

        protected virtual void DoHitEffect(GameObject target)
        {
            //TODO: override to inflict effects other than damage

            HealthEngine health = target.GetComponent<HealthEngine>();
            if (health)
            {
                health.InflictDamage(this.Model.Damage, DamageReason.DEFAULT, 0, this._owner);
            }


            if (GameVar.Ins.SelfDamage)
            {
                HealthEngine ownerHealth = this._owner.GetComponent<HealthEngine>();
                if (ownerHealth != null) ownerHealth.InflictDamage(this.Model.Damage, DamageReason.SELF_DAMAGE, 0, null);
            }
        }

        public virtual GameObject[] Target
        {
            get
            {
                List<GameObject> targets = new List<GameObject>();
                RaycastHit hit;
                Ray ray = new Ray(this._eye.transform.position, this._eye.transform.forward);
                float weaponRange = this.Model.ShotRange;

                if (Physics.Raycast(ray, out hit, weaponRange))
                {
                    targets.Add(hit.collider.gameObject);
                }

                return targets.ToArray();
            }
        }

        protected virtual bool CanShoot => this._gunState == GunState.READY_TO_FIRE;

        protected void PublishAmmoChange()
        {
            EventCenter.Publish(
                EventId.WEAPON_AMMO_CHANGE,
                new PubData.WeaponAmmoChange(this.Owner, this.Model.RemainAmmo, this.Model.TotalAmmo)
            );
        }

        protected void RecalulateAmmo()
        {
            if (this.Model.RemainAmmo == 0)
            {
                this._gunState = GunState.AMMO_OUT;
                this.DoReload();
            }
            else this.DoRecoil();
        }

        protected virtual void DoReload()
        {
            if (this.Model.TotalAmmo <= 0 || this.Model.RemainAmmo == this.Model.MagazineSize) return;

            this._gunState = GunState.RELOADING;
            int ammoInLastMag = this.Model.RemainAmmo;
            this.gameObject.PlaySound(_soundReload);
            this.RunAnimUnequip();

            LeanTween.delayedCall(
                Mathf.Max(Time.fixedDeltaTime, this.Model.ReloadTime),
                () =>
                {
                    if (!this.IsEquiped) return;

                    this.RunAnimEquip();
                    this.gameObject.PlaySound(_soundEquip);

                    this._equipmentObject.SetActive(true);
                    int totalAmmo = this.Model.TotalAmmo + this.Model.RemainAmmo;
                    int nextMagazine = Math.Min(this.Model.MagazineSize, totalAmmo);

                    this.Model.RemainAmmo = nextMagazine;
                    this.Model.TotalAmmo = totalAmmo - nextMagazine;
                    this._gunState = GunState.READY_TO_FIRE;
                    this.PublishAmmoChange();
                }
            );
        }

        protected virtual void DoRecoil()
        {
            this._gunState = GunState.RECOIL;
            LeanTween.delayedCall(
                1 / this.Model.FireRate,
                () => this._gunState = GunState.READY_TO_FIRE
            );
        }

        public virtual void TriggerAttack()
        {
            if (this.IsReady && this.CanShoot)
            {
                this.Shoot();
            }
            else if (this.NeedReload())
            {
                this.TriggerReload();
            }
        }

        public virtual void TriggerReload()
        {
            this.DoReload();
        }

        public virtual bool NeedReload()
        {
            return (float)this.Model.RemainAmmo / this.Model.MagazineSize <= 0.3;
        }
    }

    public enum GunState
    {
        AMMO_OUT,
        READY_TO_FIRE,
        RECOIL,
        RELOADING,
    }
}
