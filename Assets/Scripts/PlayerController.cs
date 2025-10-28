using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerController : MonoBehaviour
{
    CharacterController controller;

    public float moveSpeed = 5.0f; //移動スピード
    public float jumpForce = 8.0f; //ジャンプパワー
    public float gravity = 20.0f; //重力
    float recoverTime = 0.0f;

    Vector3 moveDirection = Vector3.zero; //移動成分

    public GameObject body; //点滅対象
    bool isDamage; //ダメージフラグ

    //public int life = 10;
    
    //音にまつわるコンポーネントとSE音情報
    AudioSource audio;
    public AudioClip se_shot;
    public AudioClip se_damage;
    public AudioClip se_jump;
    public AudioClip se_walk;

    //足音判定
    float footstepInterval = 0.6f; //足音間隔
    float footstepTimer; //時間計測

    void Start()
    {
        //オーディオコンポーネントを取得
        audio = GetComponent<AudioSource>();
        //各コンポーネントを取得
        controller = GetComponent<CharacterController>();
        //GameManager.
        

    }

    void Update()
    {

        //もしゲームステータスがPlayingかgameClearじゃないならなにもしない
        if (GameManager.gameState != GameState.playing && GameManager.gameState != GameState.gameclear)
        {
            return;
            
        }


        //もし地面に接地していたら
        if (controller.isGrounded)
        {

            //もし〇キーが押されたら△に動く
            //if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) MoveToLeft();
            //if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) MoveToRight();
            //if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) MoveToUp();
            //if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) MoveToDown();

            //足音
            HandleFootsteps();


            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // ローカル座標系での移動方向
            moveDirection = new Vector3(horizontal, 0, vertical);

            // 正規化して速度を一定に保つ
            if (moveDirection.magnitude > 1)
            {
                moveDirection.Normalize();
            }

            moveDirection *= moveSpeed; //スピードで倍増
       

            //ジャンプ
            if (Input.GetKeyDown(KeyCode.Space)) Jump();

        }

        //もしスタン中なら
        if (IsStun())
        {
            //復活までの時間をカウント
            recoverTime -= Time.deltaTime;

            //点滅処理
            Blinking();

        }

        //重力分の力を毎フレーム追加
        moveDirection.y -= gravity * Time.deltaTime;

        //移動実行
        Vector3 globalDirection = transform.TransformDirection(moveDirection);
        controller.Move(globalDirection * Time.deltaTime);

        //接地していたらYはリセット
        if(controller.isGrounded)moveDirection.y = 0;

    }

    //public void MoveToLeft()
    //{
    //    if (IsStun()) return;
    //    SEPlay(SEType.Walk);
    //}

    //public void MoveToRight()
    //{
    //    if (IsStun()) return;
    //    SEPlay(SEType.Walk);
    //}

    //public void MoveToUp()
    //{
    //    if (IsStun()) return;
    //    SEPlay(SEType.Walk);

    //}

    //public void MoveToDown()
    //{
    //    if (IsStun()) return;
    //    SEPlay(SEType.Walk);

    //}

    void Jump()
    {
        if (IsStun()) return;
        if (controller.isGrounded)
        {
            SEPlay(SEType.Jump);
            moveDirection.y = jumpForce;

        }
    }

    public int Life()
    {
        //return life;
        return GameManager.playerHP;
    }

    bool IsStun()
    {
        //recoverTimeが作動中かLifeが0になった場合はStunフラグがON
        //bool stun = recoverTime > 0.0f || life <= 0;
        bool stun = recoverTime > 0.0f || GameManager.playerHP <= 0;
        //StunフラグがOFFの場合はボディを確実に表示
        if (!stun) body.SetActive(true);
        //Stunフラグをリターン
        return stun;
    }

    //CharaControllerに衝突判定が生じたときの処理
    private void OnTriggerEnter(Collider hit)
    {
        if (IsStun()) return;

        //ぶつかった相手がEnemyかEnemyBulletなら
        if (hit.gameObject.CompareTag("Enemy") || hit.gameObject.CompareTag("EnemyBullet"))
        {
            SEPlay(SEType.Damage);

            //体力をマイナス
            GameManager.playerHP--;
            recoverTime = 0.5f;

            //if (life <= 0)
            if (GameManager.playerHP <= 0)
            {
                GameManager.gameState = GameState.gameover;
                Destroy(gameObject, 0.5f); //少し時間差で自分を消滅
            }
            
        }
    }

    void Blinking()
    {
        //その時のゲーム進行時間で正か負かの値を算出
        float val = Mathf.Sin(Time.time * 50);
        //正の周期なら表示
        if (val >= 0) body.SetActive(true);
        //負の周期なら非表示
        else body.SetActive(false);
    }

    //SE再生
    public void SEPlay(SEType type)
    {
        switch (type)
        {
            case SEType.Shot:
                audio.PlayOneShot(se_shot);
                break;
            case SEType.Damage:
                audio.PlayOneShot(se_damage);
                break;
            case SEType.Jump:
                audio.PlayOneShot(se_jump);
                break;
            case SEType.Walk:
                audio.PlayOneShot(se_walk);
                break;
        }
    }

    //足音
    void HandleFootsteps()
    {
        //プレイヤーが動いていれば
        if (moveDirection.x != 0 || moveDirection.z != 0)
        {
            footstepTimer += Time.deltaTime; //時間計測

            if (footstepTimer >= footstepInterval) //インターバルチェック
            {
                audio.PlayOneShot(se_walk);
                footstepTimer = 0;
            }
        }
        else //動いていなければ時間計測リセット
        {
            footstepTimer = 0f;
        }
    }

}
