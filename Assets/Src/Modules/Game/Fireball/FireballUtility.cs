using System.Collections;
using System.Collections.Generic;
using Character;
using Projectile;
using UnityEngine;
using Utilities;

namespace FireBall
{
    public class FireballUtility : EquipUtility
    {
        [SerializeField] GameObject _fireBall;
        [SerializeField] Camera _eye;

        FireballModel _model;
        public FireballModel Model => _model;

        protected override void GetModel()
        {
            this._model = this.GetComponent<FireballStats>().Model;
        }

        protected override void ActiveUtil()
        {
            GameObject newProjectile = Instantiate(_fireBall, _eye.transform.position, _eye.transform.rotation);
            newProjectile.transform.LookAt(_eye.transform.position);
            ProjectileEngine engine = newProjectile.GetComponent<ProjectileEngine>();
            engine.OnHit = () => this.ApplyHitEffect(engine.CollidedObject, this.Model.Damage);
            engine.Owner = _owner;

            base.ActiveUtil();
        }

        void ApplyHitEffect(GameObject gameObject, float damage)
        {
            HealthEngine health = gameObject.GetComponent<HealthEngine>();
            if (health)
            {
                health.InflictDamage(damage);
            }
        }
    }
}