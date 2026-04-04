using UnityEngine;

public class PlayerInputFrame
{
    // 轴向输入：用于移动和方向判定。
    public float moveX { get; private set; }
    public float moveY { get; private set; }

    // 一次性按下事件：在当前帧内由各状态/系统消费。
    public bool jumpPressed { get; private set; }
    public bool counterPressed { get; private set; }
    public bool dashPressed { get; private set; }
    public bool magicPressed { get; private set; }
    public bool ultimatePressed { get; private set; }
   
    

    public bool quickItem1UsedPressed { get; private set; }

    public bool quickItem2UsedPressed { get; private set; }
    // 鼠标按键事件：用于普攻、投剑确认与取消。
    public bool primaryAttackPressed { get; private set; }
    public bool secondaryAttackPressed { get; private set; }
    public bool secondaryAttackReleased { get; private set; }

    
    // 鼠标屏幕坐标：供需要屏幕转世界坐标的逻辑读取。
    public Vector2 mouseScreenPosition { get; private set; }

    public void Capture()
    {
        // 所有 Player 输入统一在这里采集，避免分散读取 Input。
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");

        jumpPressed = Input.GetKeyDown(KeyCode.Space);
        counterPressed = Input.GetKeyDown(KeyCode.Q);
        dashPressed = Input.GetKeyDown(KeyCode.LeftShift);
        magicPressed = Input.GetKeyDown(KeyCode.E);
        ultimatePressed = Input.GetKeyDown(KeyCode.R);
        
        
        quickItem1UsedPressed = Input.GetKeyDown(KeyCode.Alpha1);
        quickItem2UsedPressed = Input.GetKeyDown(KeyCode.Alpha2);


        primaryAttackPressed = Input.GetMouseButtonDown(0);
        secondaryAttackPressed = Input.GetMouseButtonDown(1);
        secondaryAttackReleased = Input.GetMouseButtonUp(1);

        mouseScreenPosition = Input.mousePosition;
    }
    public void Clear()
    {
        moveX = 0f;
        moveY = 0f;

        jumpPressed = false;
        counterPressed = false;
        dashPressed = false;
        magicPressed = false;
        ultimatePressed = false;
       
        quickItem1UsedPressed = false;
        quickItem2UsedPressed = false;

        primaryAttackPressed = false;
        secondaryAttackPressed = false;
        secondaryAttackReleased = false;

        
    }
}
