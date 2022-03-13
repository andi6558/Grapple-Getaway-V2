using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Platformer.Mechanics
{
    public class RopeSystem : MonoBehaviour
    {

        public float climbSpeed = 3f;
        private bool isColliding;
        const int maxGrappleTime = 60;
        const int maxGrappleCooldown = 1250;
        const float ImpulseMult = 8;
        private int cooldownTimer;
        private int cooldownTimer2;
        private int grappleTime;
        private float zipMult = 3;
        private bool reeling = false;

        public GameObject ropeHingeAnchor;

        // public GameObject particleSystem;
        public DistanceJoint2D ropeJoint;
        public Transform crosshair;
        public SpriteRenderer crosshairSprite;
        public PlayerMovement playerMovement;
        private bool ropeAttached;
        private Vector2 playerPosition;
        private Rigidbody2D ropeHingeAnchorRb;
        private SpriteRenderer ropeHingeAnchorSprite;

        private float lastJump;


        public LineRenderer ropeRenderer;
        public LayerMask ropeLayerMask;
        private float ropeMaxCastDistance = 10f;
        private List<Vector2> ropePositions = new List<Vector2>();


        private bool distanceSet;

        SpriteRenderer sr;
        Color defaultColor;
        Color defaultLineColor;

        // private ParticleSystem ps;

        private int particleTimer;
        private const int particleDuration = 75;


        void Awake()
        {

            ropeJoint.enabled = false;
            playerPosition = transform.position;
            ropeHingeAnchorRb = ropeHingeAnchor.GetComponent<Rigidbody2D>();
            ropeHingeAnchorSprite = ropeHingeAnchor.GetComponent<SpriteRenderer>();
            sr = GetComponent<SpriteRenderer>();
            defaultColor = sr.color;
            // ps = particleSystem.GetComponent<ParticleSystem>(); // Stores the module in a local variable
            // var emission = ps.emission;
            // emission.enabled = false;
        }

        void Update()
        {
            var worldMousePosition =
                Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
            var facingDirection = worldMousePosition - transform.position;
            var aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
            if (aimAngle < 0f)
            {
                aimAngle = Mathf.PI * 2 + aimAngle;
            }


            var aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.right;

            playerPosition = transform.position;


            if (!ropeAttached)
            {
                playerMovement.isSwinging = false;
                SetCrosshairPosition(aimAngle);

            }
            else
            {
                playerMovement.isSwinging = true;
                playerMovement.ropeHook = ropePositions.Last();
                crosshairSprite.enabled = false;
            }

            if (ropeAttached && grappleTime == -1 && ropeHingeAnchorRb.transform.position.y < transform.position.y - 2.0) {
                ResetRope("Horizontal failure Impulse");
            }

            HandleInput(aimDirection);
            lastJump = Input.GetAxis("Jump");
            UpdateRopePositions();
            UpdatePlayerColor();
            HandleRopeLength();
            if(grappleTime >= 0)
                grappleTime -= 1;
            if(grappleTime == 0) {
                ResetRope("Grapple End Impulse");
            }
            if(cooldownTimer >= 1)
                cooldownTimer -= 1;
            if(particleTimer >= 1)
            {
                particleTimer -= 1;
                if(particleTimer == 0)
                {
                    // var emission = ps.emission;
                    // emission.enabled = false;
                    sr.color = defaultColor; // Applies the new value directly to the Particle System
                }
            }
        }

        private void SetCrosshairPosition(float aimAngle)
        {
            if (!crosshairSprite.enabled)
            {
                crosshairSprite.enabled = true;
            }

            var x = transform.position.x + 1f * Mathf.Cos(aimAngle);
            var y = transform.position.y + 1f * Mathf.Sin(aimAngle);

            var crossHairPosition = new Vector3(x, y, 0);
            crosshair.transform.position = crossHairPosition;
        }

        private void particleEffect() {
            // var emission = ps.emission;
            // emission.enabled = true; // Applies the new value directly to the Particle System
            // emission.rate = 4000;
            // particleTimer = particleDuration;
            // Debug.Log("Right?");
            sr.color = Color.yellow;
        }

        private void UpdatePlayerColor() {
            
            float ratio = (float)(cooldownTimer * 1.0f / maxGrappleCooldown);
            // Debug.Log($"{cooldownTimer}, {maxGrappleCooldown}, {ratio}");
            sr.color = ratio * Color.red + (1.0f - ratio) * defaultColor;

            if(ropeAttached && grappleTime == -1 && ropeHingeAnchorRb.transform.position.y < transform.position.y) {
                 float x = transform.position.y - ropeHingeAnchorRb.transform.position.y;
                 ratio = x / 2.0f;
                 Color linecolor = ratio * Color.red + (1.0f - ratio) * Color.white;
                 ropeRenderer.SetColors(linecolor,linecolor);
            }

            if(cooldownTimer == 1)
                particleEffect();


            
        }

        
        private void HandleInput(Vector2 aimDirection)
        {
            if (Input.GetMouseButton(0))
            {
                if(cooldownTimer2 != 0)
                    return;
                cooldownTimer2 = 1;
                if (ropeAttached)
                    ResetRope("Cancel Swing Impulse");
                ropeRenderer.enabled = true;

                var hit = Physics2D.Raycast(playerPosition, aimDirection, ropeMaxCastDistance, ropeLayerMask);

                if (hit.collider != null)
                {
                    ropeAttached = true;
                    if (!ropePositions.Contains(hit.point))
                    {

                        ropePositions.Add(hit.point);
                        ropeJoint.distance = Vector2.Distance(playerPosition, hit.point);
                        ropeJoint.enabled = true;
                        ropeHingeAnchorSprite.enabled = true;
                    }
                }
                
                else
                {
                    ropeRenderer.enabled = false;
                    ropeAttached = false;
                    ropeJoint.enabled = false;
                }
            }
            else {
                if(cooldownTimer2 >= 1)
                    cooldownTimer2 -= 1;
            }


            if (Input.GetMouseButton(1))
            {
                if(cooldownTimer != 0) 
                    return;
                cooldownTimer = maxGrappleCooldown;
                reeling = true;
                grappleTime = maxGrappleTime;
                if (ropeAttached && grappleTime == -1)
                    ResetRope("Cancel Swinging Implse");
                ropeRenderer.enabled = true;

                var hit = Physics2D.Raycast(playerPosition, aimDirection, ropeMaxCastDistance * 1.5f, ropeLayerMask);


                if (hit.collider != null)
                {
                    ropeAttached = true;
                    if (!ropePositions.Contains(hit.point))
                    {
                        ropePositions.Add(hit.point);
                        ropeJoint.distance = Vector2.Distance(playerPosition, hit.point);
                        ropeJoint.enabled = true;
                        ropeHingeAnchorSprite.enabled = true;
                    }
                }

                else
                {
                    grappleTime = -1;
                    ropeRenderer.enabled = false;
                    ropeAttached = false;
                    ropeJoint.enabled = false;
                    reeling = false;
                }
            }

            if (ropeAttached && Input.GetAxis("Jump") != 0 && lastJump != Input.GetAxis("Jump"))
            {
                ResetRope("Manual Jump Cancel Impulse");
            }
        }

        
        private void ResetRope(string s = "Anonymous Impulse")
        {
            ropeRenderer.SetColors(Color.white,Color.white);
            reeling = false;
            // grappleTime = maxGrappleTime;
            ropeJoint.enabled = false;
            ropeAttached = false;
            playerMovement.isSwinging = false;
            ropeRenderer.positionCount = 2;
            ropeRenderer.SetPosition(0, transform.position);
            ropeRenderer.SetPosition(1, transform.position);
            ropePositions.Clear();
            ropeHingeAnchorSprite.enabled = false;
            if(grappleTime >= 0) {
                Vector2 ropePosition = ropeHingeAnchorRb.transform.position;
                Debug.Log(s);
                grappleTime = -1;
                Vector2 playerPosition = transform.position;
                Vector2 toAnchor = (ropePosition - playerPosition).normalized;
                Rigidbody2D rBody = this.GetComponent<Rigidbody2D>();
                rBody.AddForce(toAnchor * ImpulseMult, ForceMode2D.Impulse);
            }
            if(grappleTime == -1 && ropeAttached) {
                Debug.Log(s);
                Rigidbody2D rBody = this.GetComponent<Rigidbody2D>();
                var x = rBody.velocity.x;
                var y = Vector2.up;
                rBody.AddForce(y * x * 2f, ForceMode2D.Impulse);
            }
        }


        private void UpdateRopePositions()
        {
            // 1
            if (!ropeAttached)
            {
                return;
            }

            // 2
            ropeRenderer.positionCount = ropePositions.Count + 1;

            // 3
            for (var i = ropeRenderer.positionCount - 1; i >= 0; i--)
            {
                if (i != ropeRenderer.positionCount - 1) // if not the Last point of line renderer
                {
                    ropeRenderer.SetPosition(i, ropePositions[i]);

                    // 4
                    if (i == ropePositions.Count - 1 || ropePositions.Count == 1)
                    {
                        var ropePosition = ropePositions[ropePositions.Count - 1];
                        if (ropePositions.Count == 1)
                        {
                            ropeHingeAnchorRb.transform.position = ropePosition;
                            if (!distanceSet)
                            {
                                ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                                distanceSet = true;
                            }
                        }
                        else
                        {
                            ropeHingeAnchorRb.transform.position = ropePosition;
                            if (!distanceSet)
                            {
                                ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                                distanceSet = true;
                            }
                        }
                    }
                    // 5
                    else if (i - 1 == ropePositions.IndexOf(ropePositions.Last()))
                    {
                        var ropePosition = ropePositions.Last();
                        ropeHingeAnchorRb.transform.position = ropePosition;
                        if (!distanceSet)
                        {
                            ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                }
                else
                {
                    // 6
                    ropeRenderer.SetPosition(i, transform.position);
                }
            }
        }

        private void HandleRopeLength()
        {
            // 1
            if (Input.GetAxis("Vertical") >= 1f && ropeAttached)
            {
                ropeJoint.distance -= Time.deltaTime * climbSpeed;
            }
            else if (Input.GetAxis("Vertical") < 0f && ropeAttached)
            {
                ropeJoint.distance += Time.deltaTime * climbSpeed;
            }
            else if (grappleTime > 0 && reeling)
            {
                ropeJoint.distance -= Time.deltaTime * climbSpeed * zipMult;
            }
        }

        void OnTriggerStay2D(Collider2D colliderStay)
        {
            isColliding = true;
        }

        private void OnTriggerExit2D(Collider2D colliderOnExit)
        {
            isColliding = false;
        }


    }
}
