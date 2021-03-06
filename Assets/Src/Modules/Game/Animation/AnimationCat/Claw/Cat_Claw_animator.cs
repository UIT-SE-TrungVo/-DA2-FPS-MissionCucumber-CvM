using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cat_Claw_animator : MonoBehaviour
{
    const float ATTACK_DURATION = 1f;
    bool _inAttackAnim = false;

    PhotonView view;
    Animator animator;
    [SerializeField] GameObject player_cat;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        view = this.GetComponent<PhotonView>();
        if (!view.IsMine) this.enabled = false;
        AnimatorInit();
    }
    void AnimatorInit()
    {
        animator.SetBool("isAttacking", false);
    }
    // Update is called once per frame
    void Update()
    {
        if (InputMgr.StartShoot(player_cat))
        {
            animator.SetBool("isAttacking", true);
            animator.speed = 2;
            this._inAttackAnim = true;
            LeanTween.delayedCall(ATTACK_DURATION, () =>
            {
                this._inAttackAnim = false;
                animator.speed = 1;
            });
        }
        else if (!this._inAttackAnim) animator.SetBool("isAttacking", false);
    }

}
