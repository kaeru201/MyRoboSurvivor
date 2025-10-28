# RovoSurvivor（共同製作ゲーム）
## プロジェクト概要
RovoSurvivorは4名チームで共同製作したUnityゲームです。  
タイトルの通りロボットを動かす3Dアクションゲームです。  
エネミーを無限生成するゲートを潰しながら最後にボスを撃破することを目的とした爽快なゲームです。  
  
![RoboSurvivorタイトル画像](readme_img/robo_readme_1.png)  
  

* 制作人数：4名  
* 制作期間：4日間  
* 使用エンジン：Unity Editorバージョン 6000.0.54f1 
* シーンレンダー：Universal 3D 
* 使用言語：C#  
* 使用アセット：（いずれもUnityアセットストアから）  
 Demo City By Versatile Studio (Mobile Friendly) →　バトルフィールド  
 Robot Hero : PBR HP Polyart　→　プレイヤーとエネミーのモデル  
 Delivery Robot　→　ボスモデル  
 Slash Effects FREE　→　エフェクト：接近攻撃のブレード  
 Free Quick Effects Vol. 1 →　エネミーを生産するゲートモデルや爆発・炎のエフェクト
* Fontデータ： Noto Sans JP-Medium SDF (TMP_Font Asset)  
* そのほかの使用ツール：GitHub、SorceTree、VisualStudio、Googleドライブ(スプレッドシート)で情報共有

## サンプルゲーム
ぜひゲームを体験してみてください！  
[RoboSurvivorサンプル](https://dynarise2001.xyz/kunren/sample/robosurvivor_bravo/)

## ゲームフロー
* タイトル  
![タイトル画面](readme_img/robo_readme_2.png)  
  
* オープニングシーン  
ストーリーの説明をしながらこれからバトルが始まるという緊張感を高めます  
![オープニング画面](readme_img/robo_readme_3.png)  

* ゲームステージ（操作パート）  
街中のエネミーを殲滅します！  
画面右の指示にしたがって行動をすることになります。  
まずはエネミーを無限に生成するゲートを叩き、生き残ったエネミーを殲滅するといよいよボスステージです！  
![ゲームステージ画像](readme_img/robo_readme_4.png)  
  
* ボスステージ（操作パート）  
ボスステージに移行すると巨大なボスロボとのバトルになります。  
ボスは距離に応じた複数のアクションをとるので、行動を見極めながら攻撃を叩きこみます。  
BOSSのライフを0にすることができたらボス撃破！エンディングシーンへと続きます。  
![ボスステージ画像](readme_img/robo_readme_5.png)  

* エンディングシーン  
エンディングでは悲しげなBGMにあわせて、ひとときの休息を得たことを告げるテキストを読み上げます。  
カメラワークが最後に天を見上げたところでタイトルに戻ります。  
![エンディング画像](readme_img/robo_readme_6.png)  
  
## 操作方法
前後・左右　移動：WASDキー、または矢印キー  
ジャンプ：スペースキー  
プレイヤーの視点：マウス  
ショット攻撃：マウス左クリック　※弾数制限あるので打ちすぎ注意  
接近攻撃（ブレード）：マウス右  
  
プレイヤーにもライフがあり、ライフ0でゲームオーバーとなるので注意！  
![UI説明](readme_img/robo_readme_7.png)  
  
## 共同製作における主な担当パート
共同製作では主に敵の制御を担当しました。  
エネミーオブジェクトに対してEnemyController.csスクリプトを構築、難しすぎず簡単になりすぎない難易度を目指しました。  
  
### 敵の距離に応じた挙動  
エネミーにはプレイヤーとの距離に応じた挙動になるように調整しました。
* 探索圏外ならなにもしない
* 探索圏内になったらプレイヤーの方を追う
* 射程圏内になったら動きをゆっくりにして、プレイヤーに向かって弾を発射する
* 接近限界距離内になったら足を止める
それぞれをif文で確かめて、それぞれの距離に応じた挙動になるようにしました。

  
```C#
void Update()
    {
        //playing以外なら何もしないまたはplayerがいないなら
        if (GameManager.gameState != GameState.playing || player == null)
        {            
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
    }
```
  
### プレイヤーに向かって弾を発射したときの挙動
チームで話をしたときに、敵の弾が強すぎるため調整したほうが良いということになりました。
そこで、エネミーが弾を発射するときに少しプレイヤーの方を向く行動を止めて発射し終わったら再度向くようにしました。
こうすることで敵の弾を避けやすくなり、ゲームが難しすぎないようになりました。
  

```C#

 void Update()
    {
       //lookOnがtrueなら
        if (lockOn)
        {
            transform.LookAt(playerPos);//playerの方を向く
        }
    }

    IEnumerator Ataack()
    {
        isAttack = true;
        lockOn = false;//playerの方を向かなくなる
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

    


```
## 共同開発におけるレビューを行いブラッシュアップ
まずは最初の2日間でプロトタイプを完成させるために担当箇所を構築しました。  
それぞれの担当箇所をGitHubを活用してマージし、当日デバッグに回れるメンバーでデバッグプレイしてプロトタイプへの評価を行いました。  
この評価に関してチームミーティングを行い、改善点と改善方法を定め残りの2日間で調整を行うことでブラッシュアップできました。  
  
![デバッグ評価](readme_img/team_review_b.png)  
  
## GitHub上でのマージ作業を意識して担当範囲に最新の注意
スピードが要求される制作期間において、マージ作業で大きなトラブルを生まないようチームの取り決めを忠実に順守しました。  
GitHubのIssuesを通じて進捗や問題点については随時チームへの報告や問題提起を行っています。
![issuesにおける情報共有](readme_img/team_issues_b.png)  
  
SourceTreeでブランチを分けてコンクリフト衝突がおこらないよう細心の注意を払いました。  
また定期的なコミットを通してバックアップも万全にしました。  
![SourceTreeの様子](readme_img/team_sourcetree_b.png)  
  
## 共同開発に関する工夫
### 仕様から反れていないかの確認作業
チームの打ち合わせで大体の方向性・仕様はあったものの、細かい部分は自身の考えに委ねられる環境でした。  
私の場合はとにかくユーザーが爽快に何回でもバトルしてみたくなる手応えを大事にしましたので、SEによる臨場感やシーン切り替えのタイミングなどを気にしました。  
一方でこだわった結果、チームとして想定された仕様や方向性から逸脱していないかも心配な部分でしたので、疑問に思った部分はチームリーダーにマメに確認をとり、マージする際の影響なども考えながら慎重に改良を重ねることができました。  
  
例えば、当時の心配ごととして、気軽に斬撃で勝てないよう近づきすぎるとボス本体からダメージ判定をもらうのを是としていたのですが、これが仕様上問題ないかはきちんと確認をいれました。  
  
### 細かいコミット作業
とにかく自分のデータにトラブルがあると、全体に影響が出てしまうので何か大きな変更を行う際にはコミットによるバージョン管理によって、いつでももとに戻れるように気を使いました。  
またコミットだけではなくプッシュを意識してクラウドにバックアップが常にある状態の維持に努めました。  
  
特にボスがタックルしてくるコルーチン制作には調整に大変苦労しましたので、コードにコメントを多く残すと同時に確実にコミットとプッシュしていくよう意識しました。
  
### 納期の意識
チーム開発ということで自分のせいで周りに影響がでないよう良い意味でプレッシャーを感じていたのですが、それ以上にこれをプレイするユーザーを意識して時間内に必ず間に合わせるという意識を大切にしました。そのために何日に何ができていないといけないという逆算に加え、さらに半日～1日余裕をもたせるようにスケジューリングしました。  
詰まってしまったところは、自分でこだわる部分とそうでない部分を「納期に間に合うか」で天秤にかけることで、リーダーに助力を乞うタイミングは基準を決めやすかったです。  
  
具体的にはボスのバリア展開について細部がどうしても腑に落ちず、自分でも追及したい気持ちが高かったのですが中間のプロトタイプまで時間が迫っていたので、逆に早めにリーダーに確認をすることとし、その代わり解説してもらった原理は徹底的に理解するように努めました。  
  
## 今後の課題
他の担当者のパートも含めゲーム全体の内容を理解はしていますが、上空を飛び交うヘリのようなNavMeshAgentコンポーネントを活用しづらいコードについては未挑戦なので、ぜひ次回以降に作ってみようと思います。  
調べたところ、いくつかの方法のうちエネミーのルーティングを行うのが一番自然だと感じましたので挑戦してみます。  
[参考サイト：Unity入門の森/移動経路の構築とOnDrawGizmosによる移動経路の可視化](https://feynman.co.jp/unityforest/game-create-lesson/tower-defense-game/enemy-route/)
  
![ヘリのイメージ映像](readme_img/future_image.png)  