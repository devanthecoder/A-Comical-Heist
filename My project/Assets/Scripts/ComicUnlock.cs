using UnityEngine;

public class ComicUnlock : MonoBehaviour
{
    public enum UnlockType
    {
        SnakeLight,
        Bridge,
        Overcharge,
        EscapeSequence
    }

    public UnlockType unlockType;
    public GameObject escapeSequence;
    public Animator escapeAnimator;
    public string escapeTrigger = "StartEscape";
    public AudioSource escapeAudio;
    public ParticleSystem[] escapeEffects;
    public GameObject tutorialUI; // Reference to the tutorial UI GameObject

    private void Reset()
    {
        if (gameObject.name == "Comic2") unlockType = UnlockType.Bridge;
        else if (gameObject.name == "Comic3") unlockType = UnlockType.Overcharge;
        else if (gameObject.name == "Comic4") unlockType = UnlockType.EscapeSequence;
    }

    private void Awake()
    {
        if (unlockType != UnlockType.EscapeSequence) return;

        if (escapeSequence == null)
        {
            escapeSequence = GameObject.Find("EscapeSequence");
        }

        if (escapeSequence != null)
        {
            escapeSequence.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null || !GrantUnlock(player)) return;

        gameObject.SetActive(false);

        if (tutorialUI != null)
        {
            tutorialUI.SetActive(true);
        }
    }

    private bool GrantUnlock(PlayerMovement player)
    {
        switch (unlockType)
        {
            case UnlockType.SnakeLight:
            case UnlockType.Bridge:
                BendingLightBeam beam = player.GetComponentInChildren<BendingLightBeam>(true);
                if (beam == null) return MissingAbility("BendingLightBeam");

                if (unlockType == UnlockType.SnakeLight) beam.UnlockBending();
                else
                {
                    beam.UnlockBridge();
                    GameObject bridgeGate = GameObject.Find("SensorDoor (2)");
                    if (bridgeGate != null) bridgeGate.SetActive(false);
                }
                return true;

            case UnlockType.Overcharge:
                OverchargerScript overcharger = player.GetComponentInChildren<OverchargerScript>(true);
                if (overcharger == null) return MissingAbility("OverchargerScript");

                overcharger.UnlockOvercharge();
                return true;

            case UnlockType.EscapeSequence:
                if (escapeSequence == null) return MissingAbility("EscapeSequence reference");

                escapeSequence.SetActive(true);
                if (escapeAnimator != null && !string.IsNullOrEmpty(escapeTrigger))
                {
                    escapeAnimator.SetTrigger(escapeTrigger);
                }
                if (escapeAudio != null) escapeAudio.Play();
                if (escapeEffects != null)
                {
                    foreach (ParticleSystem effect in escapeEffects)
                    {
                        if (effect != null) effect.Play(true);
                    }
                }
                return true;

            default:
                return false;
        }
    }

    private bool MissingAbility(string abilityName)
    {
        Debug.LogError($"{name} cannot grant its unlock because {abilityName} was not found on the player.", this);
        return false;
    }
}