using UnityEngine;
using System.Collections;

public class AnimatedReticle : MonoBehaviour
{
    private Coroutine m_AnimationCoroutine;

    private void OnEnable()
    {
        if (m_AnimationCoroutine != null)
        {
            StopCoroutine(m_AnimationCoroutine);
            m_AnimationCoroutine = null;
        }

        m_AnimationCoroutine = StartCoroutine(RunAnimationCoroutine());
    }

    private void OnDisable()
    {
        if (m_AnimationCoroutine != null)
        {
            StopCoroutine(m_AnimationCoroutine);
            m_AnimationCoroutine = null;
        }
    }

    private IEnumerator RunAnimationCoroutine()
    {
        float teleportDelay = 0.5f;
        double startTime = Time.time;

        Vector3 fromVector = new Vector3( 0.001f, 0.001f, 0.001f );
        float toSize = 0.5f;
        float lerpT = 0f;

        while ((Time.time - startTime) <= teleportDelay)
        {
            lerpT = (float)(Time.time - startTime) / teleportDelay;

            transform.localScale = Vector3.Lerp(
                fromVector,
                new Vector3(toSize, 0.025f, toSize),
                lerpT);

            yield return null;
        }

        m_AnimationCoroutine = null;
    }
}
