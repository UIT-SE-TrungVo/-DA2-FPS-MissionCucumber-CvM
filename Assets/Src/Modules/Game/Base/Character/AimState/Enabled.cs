using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character
{
    namespace AimState
    {
        public class Enabled : BaseState
        {
            Camera Eye => ((AimEngine)this._stateMachine).Eye;
            Vector2 _pAimOnScreen;

            public Enabled(StateMachine stateMachine) : base(stateMachine)
            {
                _pAimOnScreen = new Vector2(0.5f, 0.5f);
            }

            public override void LogicUpdate()
            {
                if (InputMgr.Shoot)
                {
                    this.Shoot();
                }
                else if (InputMgr.StartInteract)
                {
                    this.Interact();
                }

                base.LogicUpdate();
            }

            void Shoot()
            {
                var target = this.Target;
                if (target != null)
                {

                }
            }

            void Interact()
            {
                var target = this.Target;
                if (target != null)
                {
                    Interactable.InteractEngine interactEngine = target.GetComponent<Interactable.InteractEngine>();
                    if (interactEngine != null) interactEngine.DoInteract(this._gameObject);
                }
            }

            GameObject Target
            {
                get
                {
                    float screenX = Screen.width * this._pAimOnScreen.x;
                    float screenY = Screen.height * this._pAimOnScreen.y;
                    RaycastHit hit;
                    Ray ray = this.Eye.ScreenPointToRay(new Vector2(screenX, screenY));

                    if (Physics.Raycast(ray, out hit))
                    {
                        return hit.collider.gameObject;
                    }
                    else return null;
                }
            }
        }
    }
}