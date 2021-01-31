using UnityEngine;
using VHS;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(FirstPersonController))]
public class PlayerSounds : MonoBehaviour {
    public AudioClip[] footstepSounds;
    public AudioClip jumpSound;
    public AudioClip landSound;

    private bool _wasGrounded = true;

    private int _footstepIndex = 0;
    private AudioSource _audioSource;
    private FirstPersonController _controller;

    private void Start() {
        _audioSource = GetComponent<AudioSource>();
        _controller = GetComponent<FirstPersonController>();
    }

    private void Update() {
        if (_controller.m_isGrounded && _controller.m_currentSpeed > 1f) {
            if (!_audioSource.isPlaying) {
                _footstepIndex = (_footstepIndex + 1) % footstepSounds.Length;
                _audioSource.clip = footstepSounds[_footstepIndex];
                _audioSource.Play();
            }
        }

        if (_controller.m_isGrounded != _wasGrounded) {
            _wasGrounded = _controller.m_isGrounded;
            if (_controller.m_isGrounded) {
                _audioSource.PlayOneShot(landSound);
            }
        }

        if (_controller.m_isGrounded && _controller.movementInputData.JumpClicked) {
            _audioSource.PlayOneShot(jumpSound);
        }
    }
}
