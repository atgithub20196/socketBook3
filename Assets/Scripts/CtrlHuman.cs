using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlHuman : BaseHuman
{
    //new 关键字可以显式的隐藏从基类继承的成员。隐藏继承的成员时，该成员的派生版本将替换基类的版本。
    //虽然可以在不使用new修饰符的情况下隐藏成员，但会生成警告。
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            if (hit.collider.tag == "Terrain")
            {
                MoveTo(hit.point);
                //发送协议
                string sendStr = "Move|";
                sendStr += NetManager.GetDesc() + ",";
                sendStr += hit.point.x + ",";
                sendStr += hit.point.y + ",";
                sendStr += hit.point.z + ",";
                NetManager.Send(sendStr);
                //NetManager.sendDone.WaitOne();
            }
        }
        //攻击
        if (Input.GetMouseButtonDown(1))
        {
            if (isAttacking) return;
            if (isMoving) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            if (hit.collider.tag != "Terrain") return;
            transform.LookAt(hit.point);
            Attack();
            //发送协议
            string sendStr = "Attack|";
            sendStr += NetManager.GetDesc() + ",";
            sendStr += transform.eulerAngles.y + ",";
            NetManager.Send(sendStr);
            //攻击判定
            Vector3 lineEnd = transform.position + 0.5f * Vector3.up;
            Vector3 lineStart = lineEnd + 20 * transform.forward;
            if (Physics.Linecast(lineStart, lineEnd, out hit))
            {
                GameObject hitObj = hit.collider.gameObject;
                if (hitObj == gameObject)
                    return;
                SyncHuman h = hitObj.GetComponent<SyncHuman>();
                if (h == null)
                    return;
                sendStr = "Hit|";
                sendStr += NetManager.GetDesc() + ",";
                sendStr += h.desc + ",";
                NetManager.Send(sendStr);
            }
        }
    }
}
