using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;
using Weapons;
using UnityEngine.UI;
using PubData;
using System;

namespace GameHud
{
    public class CanvasPlayerHud : MonoBehaviour
    {
        const float ANIM_DAMAGE_DURATION = 0.5f;

        const string PNL_HEALTH = "PnlHealth";
        const string PNL_WEAPON = "PnlWeapon";
        const string LB_HEALTH = "LbHealth";
        const string LB_SHIELD = "LbShield";
        const string LB_TOTAL_AMMO = "LbTotalAmmo";
        const string LB_REMAIN_AMMO = "LbRemainAmmo";
        const string PNL_UTILITY = "PnlUtilty";
        const string IMG_UTIL = "ImgUtil";
        const string LB_COOLDOWN = "LbCooldown";
        const string IMG_DAMAGED = "ImgDamaged";

        const string ENABLED = "Enabled";

        GameObject _player;

        UiHelper uiHelper = null;
        RectTransform _pnlHealth = null;
        RectTransform _pnlWeapon = null;
        Text _lbHealth = null;
        Text _lbShield = null;
        Text _lbTotalAmmo = null;
        Text _lbRemainAmmo = null;
        RectTransform _pnlUtility = null;
        Image _imgUtil = null;
        Text _lbCooldown = null;
        Image _imgDamaged = null;

        float _cooldown = 0;

        void Awake()
        {
            EventCenter.Subcribe(EventId.CREATE_PLAYER, (data) =>
            {
                this._player = GameVar.Ins.Player;
                this.GetPlayerInfo();
            });
        }

        void Start()
        {
            this.uiHelper = new UiHelper(this.gameObject);
            this._pnlHealth = uiHelper.ui[PNL_HEALTH].GetComponent<RectTransform>();
            this._pnlWeapon = uiHelper.ui[PNL_WEAPON].GetComponent<RectTransform>();
            this._lbHealth = uiHelper.ui[LB_HEALTH].GetComponent<Text>();
            this._lbShield = uiHelper.ui[LB_SHIELD].GetComponent<Text>();
            this._lbTotalAmmo = uiHelper.ui[LB_TOTAL_AMMO].GetComponent<Text>();
            this._lbRemainAmmo = uiHelper.ui[LB_REMAIN_AMMO].GetComponent<Text>();
            this._pnlUtility = uiHelper.ui[PNL_UTILITY].GetComponent<RectTransform>();
            this._imgUtil = uiHelper.ui[IMG_UTIL].GetComponent<Image>();
            this._lbCooldown = uiHelper.ui[LB_COOLDOWN].GetComponent<Text>();
            this._imgDamaged = uiHelper.ui[IMG_DAMAGED].GetComponent<Image>();

            EventCenter.Subcribe(EventId.WEAPON_AMMO_EQUIP, this.LoadWeaponInfo);
            EventCenter.Subcribe(EventId.WEAPON_AMMO_CHANGE, this.UpdateRemainAmmo);
            EventCenter.Subcribe(EventId.WEAPON_UNEQUIP, this.HideWeaponPanel);
            EventCenter.Subcribe(EventId.HEALTH_CHANGE, this.UpdatePlayerHealth);
            EventCenter.Subcribe(EventId.SHILED_CHANGE, this.UpdatePlayerShield);
            EventCenter.Subcribe(EventId.UTILITY_CHARACTER_OWN, this.ShowUtilityPanel);
            EventCenter.Subcribe(EventId.UTILITY_START_COOLDOWN, this.UpdateUtilityCooldown);

            this._pnlUtility.gameObject.SetActive(false);
            this._imgDamaged.gameObject.SetActive(false);
        }

        void Update()
        {
            if (this._player == null && GameVar.Ins.Player != null)
            {
                this._player = GameVar.Ins.Player;
                this.GetPlayerInfo();
            }
        }

        void GetPlayerInfo()
        {
            CharacterStats stats = this._player.GetComponent<CharacterStats>();
            if (stats != null)
            {
                int health = Mathf.RoundToInt(stats.HealthModel.Health);
                int shield = Mathf.RoundToInt(stats.HealthModel.Shield);
                this._lbHealth.text = health.ToString();
                this._lbShield.text = shield.ToString();
            }
            else
            {
                UnityEngine.Debug.LogErrorFormat("[{0}] cannot get stats", this);
                Destroy(this);
            }
        }

        void LoadWeaponInfo(object pubData)
        {
            PubData.WeaponAmmoEquip data = pubData as PubData.WeaponAmmoEquip;
            if (data.Dispatcher == this._player)
            {
                this._pnlWeapon.gameObject.SetActive(true);
                this._lbRemainAmmo.gameObject.SetActive(true);
                this._lbTotalAmmo.gameObject.SetActive(true);
                this._lbRemainAmmo.text = data.AmmoInMag.ToString();

                if (data.AmmoRemain > 0) this._lbTotalAmmo.text = "/" + data.AmmoRemain.ToString();
                else this._lbTotalAmmo.text = "";
            }
        }

        void UpdateRemainAmmo(object pubData)
        {
            WeaponAmmoChange data = pubData as WeaponAmmoChange;
            if (data.Dispatcher == this._player)
            {
                this._lbRemainAmmo.text = data.AmmoInMag.ToString();

                if (data.AmmoRemain > 0) this._lbTotalAmmo.text = "/" + data.AmmoRemain.ToString();
                else this._lbTotalAmmo.text = "";
            }
        }

        void HideWeaponPanel(object pubData)
        {
            WeaponUnequip data = pubData as WeaponUnequip;
            if (data.Dispatcher == this._player)
            {
                this._lbRemainAmmo.gameObject.SetActive(false);
                this._lbTotalAmmo.gameObject.SetActive(false);
            }
        }

        void UpdatePlayerHealth(object pubData)
        {
            HealthChange data = pubData as HealthChange;
            if (data.Dispatcher == this._player)
            {
                int displayAmount = Mathf.RoundToInt(data.NewHealth);
                this._lbHealth.text = displayAmount.ToString();

                this._imgDamaged.gameObject.SetActive(true);
                LeanTween.delayedCall(ANIM_DAMAGE_DURATION, () =>
                {
                    if (this._imgDamaged.gameObject.activeInHierarchy)
                    {
                        this._imgDamaged.gameObject.SetActive(false);
                    }
                });
            }
        }

        void UpdatePlayerShield(object pubData)
        {
            ShieldChange data = pubData as ShieldChange;
            if (data.Dispatcher == this._player)
            {
                int displayAmount = Mathf.RoundToInt(data.NewShield);
                this._lbShield.text = displayAmount.ToString();
            }
        }

        void UpdateUtilityCooldown(object pubData)
        {
            UtilityStartCooldown data = pubData as UtilityStartCooldown;
            if (data.Dispatcher == this._player)
            {
                this._cooldown = data.Cooldown;
                this._lbCooldown.gameObject.SetActive(true);
                this.DisplayCoolDown();
            }
        }

        void DisplayCoolDown()
        {
            this._imgUtil.transform.Find(ENABLED).gameObject.SetActive(this._cooldown <= 0);
            if (this._cooldown <= 0)
            {
                this._lbCooldown.gameObject.SetActive(false);
            }
            else
            {
                this._lbCooldown.text = this._cooldown.ToString();
                Invoke("DisplayCoolDown", Mathf.Min(this._cooldown, 1));
                this._cooldown--;
            }
        }

        void ShowUtilityPanel(object pubData)
        {
            GameObject character = pubData as GameObject;
            if (character == this._player)
            {
                this._pnlUtility.gameObject.SetActive(true);
            }
        }
    }
}
