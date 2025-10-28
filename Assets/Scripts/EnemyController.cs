using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{

    public int enemyHP = 5; //敵のHP
    public float enemySpeed = 5.0f; //敵のスピード
    bool isDamage; //ダメージ中フラグ

    public GameObject body; //点滅されるbody

    // [SerializeField] Transform self;
    GameObject player;
    NavMeshAgent navMeshAgent;     // NavMeshAgentコンポーネンス
    new AudioSource audio;

    public float detectionRange = 80f;     // プレイヤーを検知する距離

    bool isAttack; //攻撃中フラグ
    public float attackRange = 30f;         // 攻撃を開始する距離
    public float stopRange = 5f; //接近限界距離
    public GameObject bulletPrefab;     // 発射する弾のPrefab
    public GameObject gate;            // 弾を発射する位置
    public float bulletSpeed = 100f;    // 発射する弾の速度 
    public float fireInterval = 2.0f; //弾を発射するインターバル
    bool lockOn = true; //ターゲット

    float timer; //時間経過

    float recoverTime = 0f;

    bool death = false;

    [SerializeField] AudioClip se_Shot;
    [SerializeField] AudioClip se_Damage;
    [SerializeField] AudioClip se_Exprosion;

    [SerializeField] GameObject flamePrefab; 
    public Animator animator;
   


    GameObject gameMgr; //ゲームマネージャーusing UnityEngine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameMgr = GameObject.FindGameObjectWithTag("GM");
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player"); //生成されるたびにplayerのtransformを取得
        audio = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {


        //playing以外なら何もしないまたはplayerがいないなら
        if (GameManager.gameState != GameState.playing || player == null)
        {
            //途中でplayerが消えても止まらない；；
            return;
        }

        timer += Time.deltaTime;

        //常にPlayerの位置を取得
        Vector3 playerPos = player.transform.position;
        //常にこのオブジェクトとプレイヤーオブジェクトの距離を測る
        float playerDistance = Vector3.Distance(playerPos, transform.position);

        //プレイヤーの距離がプレイヤーを検知する距離より大きいなら
        if (playerDistance >= detectionRange)
        {
            navMeshAgent.isStopped = true;//止める
            lockOn = false; //プレイヤーの方を向くフラグをOFF
        }
        //プレイヤーの距離がプレイヤーを検知する距離内だったら
        else if (playerDistance <= detectionRange)
        {
            navMeshAgent.SetDestination(playerPos);//playerを追う
            navMeshAgent.isStopped = false;//止めるフラグOFF

            if(!isAttack) lockOn = true;//プレイヤーの方を向くフラグをON 　変更点　Attackなら止めるように変更



            //攻撃距離内だったら
            if (playerDistance <= attackRange)
            {
                //動きをゆっくり
                navMeshAgent.speed = 2.5f;

                if (timer >= fireInterval)
                {
                    if (!isAttack)
                    {

                        //アタックコルーチン発動
                        StartCoroutine(Ataack());
                    }
                }



                //もしプレイヤーの距離が接近限界距離より小さいなら
                if (playerDistance <= stopRange)
                {
                    navMeshAgent.isStopped = true;//止める
                }



            }
            else
            {
                navMeshAgent.speed = 5;//speedを通常スピードに(5）
            }

        }

        //lookOnがtrueなら
        if (lockOn)
        {
            transform.LookAt(playerPos);//playerの方を向く
        }

        if (enemyHP <= 0)
        {
            if (!death)
            {
                //死んだらEnemyListを削除
                gameMgr.GetComponent<GameManager>().enemyList.RemoveAt(0);

                //gameManager.enemyList.RemoveAt(0);
                StartCoroutine(Dead());
            }
                         
        }

        if (IsDamage())//覚えておく。IsDamageメソッド発動してからそれの値がtureなら　覚えておく。
        {
            recoverTime -= Time.deltaTime;//recoverTimeが0になるまで経過時間を引く
            Blinking();//点滅処理  
        }
    }

    IEnumerator Dead()
    {
        death = true;

        animator.SetTrigger("die");
        SEPlay(SEType.Explosion);
        Instantiate(flamePrefab, transform.position, flamePrefab.transform.rotation);

        yield return new WaitForSeconds(1);
        //自分も破壊
        Destroy(gameObject);

    }

    IEnumerator Ataack()
    {
        isAttack = true;
        lockOn = false;
        yield return new WaitForSeconds(1f); //変更点　待ってから撃つ

        GameObject obj = Instantiate(bulletPrefab, gate.transform.position, gate.transform.rotation * Quaternion.Euler(90, 0, 0));

        obj.GetComponent<Rigidbody>().AddForce(gate.transform.forward * bulletSpeed, ForceMode.Impulse);

        SEPlay(SEType.Shot);

        timer = 0;

        //Debug.Log("攻撃中だよ");
        //yield return new WaitForSeconds(1);
        isAttack = false;
        lockOn = true;　　　　　　　　　　　
        yield break;
    }


    //攻撃を食らったら(一旦triggerEnter)
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            enemyHP--;

            recoverTime = 0.5f;//当たった時

            SEPlay(SEType.Damage);//何かに当たった時点で鳴らしてもいいけど今後のことを考えてそれぞれにいれる

            Debug.Log(enemyHP);


        }
        if (collision.gameObject.CompareTag("PlayerSword"))
        {
            enemyHP = enemyHP - 3;

            recoverTime = 0.5f;//当たった時
            SEPlay(SEType.Damage);

            Debug.Log(enemyHP);
        }

    }

    //recoverが0より大きくなった瞬間にtrueになって、Updataで経過時間引いて0になった瞬間にfalseにするメソッド
    //その時間差で点滅処理をさせる
    bool IsDamage()
    {
        bool isDamage = recoverTime > 0; //recoverTimeが0以上だったら

        body.SetActive(true);//最後は表示する





        return isDamage;
    }

    //点滅処理
    void Blinking()
    {
        float val = Mathf.Sin(Time.time * 50);

        if (val >= 0) body.SetActive(true);

        else body.SetActive(false);

    }

    void SEPlay(SEType type)
    {
        switch (type)
        {
            case SEType.Shot:
                audio.PlayOneShot(se_Shot);
                break;
            case SEType.Damage:
                audio.PlayOneShot(se_Damage);
                break;
            case SEType.Explosion:
                //audio.PlayOneShot(se_Exprosion);
                AudioSource.PlayClipAtPoint(se_Exprosion, transform.position);//このやり方だと音が小さい気がします
                break;


        }
    }

}
