using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasMakeRoom : MonoBehaviour
{
    const string BTN_CREATE = "BtnCreate";
    const string BTN_JOIN = "BtnJoin";
    const string BTN_SOLO = "BtnSolo";
    const string TOOGLE_PUBLIC = "TogglePublic";
    const string TOOGLE_PRIVATE = "TogglePrivate";
    const string TXT_ROOM_CODE = "TxtRoomCode";
    const string BTN_CONFIRM = "BtnConfirm";

    UiHelper uiHelper = null;
    Button _btnCreate = null;
    Button _btnJoin = null;
    Button _btnSolo = null;
    Toggle _tooglePublic = null;
    Toggle _tooglePrivate = null;
    InputField _txtRoomCode = null;
    Button _btnConfirm = null;

    RoomMode? _currentRoomMode = null;

    enum Selection
    {
        NULL,
        CREATE,
        JOIN,
        SOLO
    }
    Selection curSelection;

    // Start is called before the first frame update
    void Start()
    {
        this.uiHelper = new UiHelper(this.gameObject);
        this._btnCreate = uiHelper.ui[BTN_CREATE].GetComponent<Button>();
        this._btnJoin = uiHelper.ui[BTN_JOIN].GetComponent<Button>();
        this._btnSolo = uiHelper.ui[BTN_SOLO].GetComponent<Button>();
        this._tooglePublic = uiHelper.ui[TOOGLE_PUBLIC].GetComponent<Toggle>();
        this._tooglePrivate = uiHelper.ui[TOOGLE_PRIVATE].GetComponent<Toggle>();
        this._txtRoomCode = uiHelper.ui[TXT_ROOM_CODE].GetComponent<InputField>();
        this._btnConfirm = uiHelper.ui[BTN_CONFIRM].GetComponent<Button>();

        this._btnCreate.onClick.AddListener(delegate { this.SetSelection(Selection.CREATE); });
        this._btnJoin.onClick.AddListener(delegate { this.SetSelection(Selection.JOIN); });
        this._btnSolo.onClick.AddListener(delegate { this.SetSelection(Selection.SOLO); });
        this._btnConfirm.onClick.AddListener(this.Confirm);

        this.SetRoomMode(RoomMode.PUBLIC);
        this.SetSelection(Selection.NULL);

        this._tooglePublic.onValueChanged.AddListener(delegate { this.SetRoomMode(RoomMode.PUBLIC); });
        this._tooglePrivate.onValueChanged.AddListener(delegate { this.SetRoomMode(RoomMode.PRIVATE); });
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetSelection(Selection selection)
    {
        this.curSelection = selection;
        this.SetStateSelected(this._btnCreate, false);
        this.SetStateSelected(this._btnJoin, false);
        this.SetStateSelected(this._btnSolo, false);

        switch (selection)
        {
            case Selection.CREATE:
                {
                    this.SetStateSelected(this._btnCreate, true);
                    break;
                }
            case Selection.JOIN:
                {
                    this.SetStateSelected(this._btnJoin, true);
                    break;
                }
            case Selection.SOLO:
                {
                    this.SetStateSelected(this._btnSolo, true);
                    break;
                }
        }
    }

    void CreateRoom()
    {
        Debug.Log("Create Room");
        CanvasRoom room = SceneMgr.GetGui(Gui.ROOM).GetComponent<CanvasRoom>();
        room.SetViewMode(RoomViewMode.HOST);
        Gm.ChangeGui(Gui.ROOM);
    }

    void JoinRoom()
    {
        string roomCode = this._txtRoomCode.text;
        if (true)
        {
            Debug.Log("Join Room" + roomCode);
            CanvasRoom room = SceneMgr.GetGui(Gui.ROOM).GetComponent<CanvasRoom>();
            room.SetViewMode(RoomViewMode.GUEST);
            Gm.ChangeGui(Gui.ROOM);
        }
    }

    void Solo()
    {
        Debug.Log("Request solo");
    }

    void SetRoomMode(RoomMode roomMode)
    {
        this._currentRoomMode = roomMode;
        Debug.Log("Set room mode: " + roomMode);
        this._tooglePublic.SetIsOnWithoutNotify(roomMode == RoomMode.PUBLIC ? true : false);
        this._tooglePrivate.SetIsOnWithoutNotify(roomMode == RoomMode.PRIVATE ? true : false);
    }

    void Confirm()
    {
        switch (this.curSelection)
        {
            case Selection.CREATE:
                {
                    this.CreateRoom();
                    break;
                }
            case Selection.JOIN:
                {
                    this.JoinRoom();
                    break;
                }
            case Selection.SOLO:
                {
                    this.Solo();
                    break;
                }
        }
        this.SetSelection(Selection.NULL);
    }

    void SetStateSelected(Button button, bool isSelected)
    {
        string PNL_SELECTED = "PnlSelected";
        Transform pnlSelected = button.gameObject.transform.Find(PNL_SELECTED);

        pnlSelected.gameObject.SetActive(isSelected);
    }
}

enum RoomMode
{
    PUBLIC,
    PRIVATE
}