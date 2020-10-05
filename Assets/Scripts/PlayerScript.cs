using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

namespace Game
{
    public class PlayerScript : NetworkBehaviour
    {
        [HideInInspector][SyncVar(hook = nameof(ChangePlayerName))] public string playerName;
        Rigidbody2D rb;
        Collider2D col;
        public Transform spriteTransform, spriteToFlip;
        public float speed, killCooldown = 20;
        Vector3 lastPos;
        [SyncVar]
        public PlayerType playerType;
        [SyncVar(hook = nameof(ChangePlayerState))]
        public PlayerState playerState;
        FieldOfView fov;
        PlayerList playerList;
        Minimap minimap;
        public List<Task> playerTasks = new List<Task>();
        public TextMesh nameText;
        [HideInInspector][SyncVar(hook = nameof(ChangeSuitColor))] public Color32 color;
        public PlayerCorpse corpsePrefab;
        [HideInInspector] public UnityAction taskAction, killAction;
        public override void OnStartClient()
        {
            base.OnStartClient();
            rb = GetComponent<Rigidbody2D>();
            if (!isLocalPlayer)
            {
                rb.isKinematic = true;
                lastPos = transform.position;
                taskAction = OtherPlayerAction;
            }
        }
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            fov = FindObjectOfType<FieldOfView>();
            col = GetComponent<Collider2D>();
            playerList = FindObjectOfType<PlayerList>();
            minimap = FindObjectOfType<Minimap>();
            PlayerStaticVars.state = clientState.showingUI;
            playerList.myPlayer = this;
            taskAction = MovementInput;
            minimap.player = transform;
            SetObjTransform(Camera.main.transform, new Vector3(0, 0, -10));
            SetPlayerName(playerName);
            StartCoroutine(StartPlayerAfterServer());
        }
        IEnumerator StartPlayerAfterServer()
        {
            yield return new WaitForSecondsRealtime(9f);
            SetPlayerFov(false);
            if (playerType != PlayerType.crewmate)
            {
                SetImpostorsName();
                taskAction += ImpostorUpdate;
            }

            PlayerStaticVars.state = clientState.showingGameplay;
            Debug.Log(playerTasks.Count + " tasks");
            foreach (Task task in playerTasks)
            {
                minimap.AddTaskToMinimap(task);
            }
        }
        void FixedUpdate()
        {
            taskAction?.Invoke();
        }
        void OtherPlayerAction()
        {
            FlipSprites(transform.position.x, lastPos.x);
            lastPos = transform.position;
        }
        public void MovementInput()
        {
            if (PlayerStaticVars.state.Equals(clientState.showingGameplay))
            {
                Walk(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
            }
        }
        void SetPlayerFov(bool isDead)
        {
            switch (isDead)
            {
                case true:
                    fov.SetDrawMeshListener(delegate { fov.DrawFullQuad(); });
                    SetFovTransform(false);
                    break;
                case false:
                    fov.SetDrawMeshListener(delegate { fov.DrawFieldOfView(); });
                    SetFovTransform(true);
                    break;
            }
        }
        void SetFovTransform(bool isAlive)
        {
            if (isAlive)
            {
                SetObjTransform(fov.transform, new Vector3(0, 0, 2));
            }
            else
            {
                fov.transform.position = fov.floor.transform.position;
                if (fov.transform.parent != null)
                {
                    fov.transform.SetParent(null);
                }
            }
        }
        void SetImpostorsName()
        {
            foreach (PlayerScript player in playerList.Impostors())
            {
                player.nameText.color = new Color32(255, 0, 0, 255);
            }
        }
        public void SetPlayerType(PlayerType type)
        {
            playerType = PlayerType.impostor;
            if (type == playerType)
            {
                return;
            }
            playerType = PlayerType.crewmate;
        }
        void SetPlayerState(PlayerState state)
        {
            playerState = state;
        }
        [Command(ignoreAuthority = true)]
        public void SetPlayerName(string playName)
        {
            playerName = playName;
        }
        void SetObjTransform(Transform objTrans, Vector3 pos)
        {
            objTrans.SetParent(transform);
            objTrans.localPosition = pos;
        }
        void Walk(Vector2 direction)
        {
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
            FlipSprites(direction.x, 0);
        }
        void FlipSprites(float x, float conditional)
        {
            if (x != conditional)
            {
                spriteToFlip.localScale = new Vector3((x > conditional) ? 1 : -1, spriteToFlip.localScale.y, spriteToFlip.localScale.z);
            }
        }
        public void SpriteEnabled(bool enabled)
        {
            spriteTransform.gameObject.SetActive(enabled);
        }
        void ImpostorUpdate()
        {
            killCooldown -= Time.deltaTime;
            ImpostorKillCheck();
        }
        public void ImpostorKillCheck()
        {
            if (Input.GetKeyDown(KeyCode.Q) && killCooldown <= 0)
            {
                if (killAction != null)
                {
                    killAction.Invoke();
                    killCooldown = 20;
                    killAction = null;
                }
            }
        }
        [Command(ignoreAuthority = true)]
        public void CmdGetKilled()
        {
            SetPlayerState(PlayerState.dead);
            SpawnCorpsePrefab();
            if (playerList == null)
            {
                playerList = FindObjectOfType<PlayerList>();
            }
            playerList.CmdCheckIfImpostorsKilledEnough();
        }
        void SpawnCorpsePrefab()
        {
            GameObject newCorpse = Instantiate(corpsePrefab.gameObject, transform.position, Quaternion.identity);
            newCorpse.GetComponent<PlayerCorpse>().ServerSetCorpse(color);
            NetworkServer.Spawn(newCorpse);
        }
        void ChangePlayerState(PlayerState oldState, PlayerState newState)
        {
            if (newState == PlayerState.dead)
            {
                GetComponentInChildren<PlayerDetection>().enabled = false;
                nameText.color = new Color32(255, 255, 255, 0);
                Renderer[] renderers = spriteTransform.GetComponentsInChildren<Renderer>();
                foreach (Renderer render in renderers)
                {
                    if (isLocalPlayer)
                    {
                        Color32 newCol = render.material.color;
                        newCol.a /= 2;
                        render.material.color = newCol;
                    }
                    else
                    {
                        render.enabled = false;
                    }
                }
                if (isLocalPlayer)
                {
                    SetPlayerFov(true);
                    gameObject.layer = 13;
                }
            }
        }
        void ChangePlayerName(string _, string newName)
        {
            nameText.text = newName;
        }
        void ChangeSuitColor(Color32 _, Color32 col)
        {
            MeshRenderer playerMesh = spriteToFlip.GetComponentInChildren<MeshRenderer>();
            playerMesh.materials[0].color = col;
        }
        public void RemoveTask(Task task)
        {
            playerTasks.Remove(task);
            minimap.RemoveTaskFromMinimap(task);
        }
    }
    public enum PlayerState
    {
        alive,
        dead,
    }
    public enum PlayerType
    {
        crewmate,
        impostor,
    }
}
