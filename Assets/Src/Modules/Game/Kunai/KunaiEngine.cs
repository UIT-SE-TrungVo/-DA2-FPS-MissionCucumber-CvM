using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapons;
using Projectile;

namespace Kunai
{
    public class KunaiEngine : AmmoWeapon
    {
        [SerializeField] GameObject _projectile;

        const string MUZZLE = "Muzzle";

        GameObject _muzzle;
        public GameObject Muzzle => _muzzle;

        public KunaiModel Model => this._model as KunaiModel;

        protected override void Awake()
        {
            base.Awake();
            _muzzle = this.transform.Find(MUZZLE).gameObject;
        }

        protected override void GetModel()
        {
            this._model = this.GetComponent<KunaiStats>().Model;
        }

        protected override void Shoot()
        {
            //create projectile
            Vector3 muzzlePoint = _muzzle.transform.position;
            Vector3 aimPoint = this.GetRayEndpoint();

            GameObject newProjectile = Instantiate(_projectile, muzzlePoint, Quaternion.identity);
            newProjectile.transform.LookAt(aimPoint);
            ProjectileEngine engine = newProjectile.GetComponent<ProjectileEngine>();
            engine.OnHit = () => base.DoHitEffect(engine.CollidedObject);
            engine.Owner = _owner;

            this._equipmentObject.gameObject.SetActive(false);
            LeanTween.delayedCall(1 / this.Model.FireRate, () =>
            {
                this._equipmentObject.gameObject.SetActive(true);
            });

            base.Shoot();
        }

        protected override void DoHitEffect(GameObject target)
        {
            //do nothing
        }

        Vector3 GetRayEndpoint()
        {
            RaycastHit hit;
            Ray ray = this._eye.ScreenPointToRay(this.AIM_POINT);
            float weaponRange = this.Model.ShotRange;

            if (Physics.Raycast(ray, out hit, weaponRange))
            {
                return hit.point;
            }
            else
            {
                return this.transform.position + ray.direction * weaponRange;
            }
        }
    }
}