using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeControler : MonoBehaviour {


    public Image img_NhatAnTat;
    public Image img_TienLenTruyenThong;
    public Image img_DemLa;

    public Image chip1;
    public Image chip2;
    public Image chip3;

    public Sprite spt_NhatAnTat_Active;
    public Sprite spt_NhatAnTat_Deactive;
    public Sprite spt_TienLenTruyenThong_Active;
    public Sprite spt_TienLenTruyenThong_Deactive;
    public Sprite spt_DemLa_Active;
    public Sprite spt_DemLa_Deactive;

    public Sprite spt_2k;
    public Sprite spt_5k;
    public Sprite spt_10k;
    public Sprite spt_20k;
    public Sprite spt_50k;
    public Sprite spt_100k;
    public Sprite spt_200k;
    public Sprite spt_500k;
    public Sprite spt_1000k;


    private GameObject obj_NhatAnTat;
    private GameObject obj_TienLenTruyenThong;
    private GameObject obj_DemLa;

    public Dropdown m_Dropdown;



    private Vector3 temppos;
    float obj_NhatAnTatPosY, obj_TienLenTruyenThongPosY, obj_DemLaPosY;
    bool isup = true;

    // Use this for initialization
    void Start () {
        obj_NhatAnTat = GameObject.Find("NhatAnTat");
        obj_TienLenTruyenThong = GameObject.Find("TLTruyenThong");
        obj_DemLa = GameObject.Find("DemLa");

        temppos = new Vector3(obj_NhatAnTat.transform.position.x - 1000f, obj_NhatAnTat.transform.position.y, obj_NhatAnTat.transform.position.z);
        obj_NhatAnTat.transform.position = temppos;
        temppos = new Vector3(obj_TienLenTruyenThong.transform.position.x - 1000f, obj_TienLenTruyenThong.transform.position.y, obj_TienLenTruyenThong.transform.position.z);
        obj_TienLenTruyenThong.transform.position = temppos;
        temppos = new Vector3(obj_DemLa.transform.position.x - 1000f, obj_DemLa.transform.position.y, obj_DemLa.transform.position.z);
        obj_DemLa.transform.position = temppos;

        obj_NhatAnTatPosY = obj_NhatAnTat.transform.position.y;
        obj_TienLenTruyenThongPosY = obj_TienLenTruyenThong.transform.position.y;
        obj_DemLaPosY = obj_DemLa.transform.position.y;

        m_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(m_Dropdown);
        });


        StartCoroutine("MoveGameMode");

    }

    // Update is called once per frame
    void Update()
    {
        if (img_NhatAnTat.tag == "Active")
        {
            img_NhatAnTat.sprite = spt_NhatAnTat_Active;
            UpDownEffect(obj_NhatAnTat, obj_NhatAnTatPosY, 0.2f);
        }
        else
        {
            img_NhatAnTat.sprite = spt_NhatAnTat_Deactive;
            //UpDownEffect(obj_NhatAnTat, obj_NhatAnTatPosY, 0f);
        }

        if (img_TienLenTruyenThong.tag == "Active")
        {
            img_TienLenTruyenThong.sprite = spt_TienLenTruyenThong_Active;
            UpDownEffect(obj_TienLenTruyenThong, obj_TienLenTruyenThongPosY, 0.2f);
        }
        else
        {
            img_TienLenTruyenThong.sprite = spt_TienLenTruyenThong_Deactive;
            //UpDownEffect(obj_TienLenTruyenThong, obj_TienLenTruyenThongPosY, 0f);
        }

        if (img_DemLa.tag == "Active")
        {
            img_DemLa.sprite = spt_DemLa_Active;
            UpDownEffect(obj_DemLa, obj_DemLaPosY, 0.2f);
        }
        else
        {
            img_DemLa.sprite = spt_DemLa_Deactive;
            //UpDownEffect(obj_DemLa, obj_DemLaPosY, 0f);
        }

    }

    void DropdownValueChanged(Dropdown change)
    {
        if (change.value == 0)
        {
            chip1.sprite = spt_2k;
            chip2.sprite = spt_5k;
            chip3.sprite = spt_10k;
        }
        if (change.value == 1)
        {
            chip1.sprite = spt_20k;
            chip2.sprite = spt_50k;
            chip3.sprite = spt_100k;
        }
        if (change.value == 2)
        {
            chip1.sprite = spt_200k;
            chip2.sprite = spt_500k;
            chip3.sprite = spt_1000k;
        }
    }


    void UpDownEffect(GameObject obj, float oripos,float power)
    {
        if (isup)
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y + power, obj.transform.position.z);
        else
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y - power, obj.transform.position.z);

        if (obj.transform.position.y > oripos + 10f)
            isup = false;
        else if (obj.transform.position.y < oripos)
            isup = true;


    }

    IEnumerator MoveGameMode()
    {
        for (int f = 0; f < 80 ; f += 1)
        {
            obj_TienLenTruyenThong.transform.position = new Vector3(obj_TienLenTruyenThong.transform.position.x + 12.5f, obj_TienLenTruyenThong.transform.position.y, obj_TienLenTruyenThong.transform.position.z);
            //obj_DemLa.transform.position
            yield return new WaitForSeconds(0.01f);
        }
        for (int f = 0; f < 40; f += 1)
        {
            obj_DemLa.transform.position = new Vector3(obj_DemLa.transform.position.x + 25f, obj_DemLa.transform.position.y, obj_DemLa.transform.position.z);
            //obj_DemLa.transform.position
            yield return new WaitForSeconds(0.01f);
        }
        for (int f = 0; f < 50; f += 1)
        {
            obj_NhatAnTat.transform.position = new Vector3(obj_NhatAnTat.transform.position.x + 20f, obj_NhatAnTat.transform.position.y, obj_NhatAnTat.transform.position.z);
            //obj_DemLa.transform.position
            yield return new WaitForSeconds(0.01f);
        }

    }

    public void Deactiveall()
    {
        img_NhatAnTat.tag = "Deactive";
        img_TienLenTruyenThong.tag = "Deactive";
        img_DemLa.tag = "Deactive";
    }

    public void ActiveGameMode(Image clickedImage)
    {
        Deactiveall();

        clickedImage.tag = "Active";
    }
}
