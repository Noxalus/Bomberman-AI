﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PlayerEvent : UnityEvent<Player> {}
[System.Serializable]
public class PlayerBonusEvent : UnityEvent<Player, EBonusType> { }

// TODO: Give the real entities list instead (this implies to create an Entity class, etc...)
[System.Serializable]
public class PlayerBombEvent : UnityEvent<List<EEntityType>> { }

public class Player : MonoBehaviour
{
    #region Constants

    // Animator key constants
    private const string ANIMATOR_RESPAWN_KEY = "Respawn";
    private const string ANIMATOR_IS_MOVING_KEY = "IsMoving";
    private const string ANIMATOR_IS_DEAD_KEY = "IsDead";
    private const string ANIMATOR_IS_INVINCIBLE_KEY = "IsInvincible";
    private const string ANIMATOR_HORIZONTAL_KEY = "Horizontal";
    private const string ANIMATOR_VERTICAL_KEY = "Vertical";
    private const string ANIMATOR_PREVIOUS_HORIZONTAL_KEY = "PreviousHorizontal";
    private const string ANIMATOR_PREVIOUS_VERTICAL_KEY = "PreviousVertical";

    #endregion

    #region Events

    [Header("Events")]

    public PlayerEvent OnPlantBomb;
    public PlayerEvent OnScoreChange;
    public PlayerEvent OnPowerChange;
    public PlayerEvent OnBombCountChange;
    public PlayerEvent OnSpeedChange;
    public PlayerEvent OnSpawn;
    public PlayerEvent OnWallDestroy;
    public PlayerEvent OnKill; // When he just kills another player
    public PlayerEvent OnDeath; // When he just gets killed
    public PlayerEvent OnDestroyed; // When the death animation is finished
    public PlayerEvent OnMove;

    public PlayerBonusEvent OnPickUpBonus;
    public PlayerBonusEvent OnBonusDestroy;

    public PlayerBombEvent OnBombExploded;

    #endregion

    #region Serialized fields

    [Header("Inner references")]

    [SerializeField] private SpriteRenderer _spriteRenderer = null;
    [SerializeField] private Animator _animator = null;
    [SerializeField] private Rigidbody2D _rigidbody = null;

    [Header("Assets")]

    [SerializeField] private GameSettings _gameSettings = null;

    #endregion

    #region Private fields

    private int _id = 0;
    private Color _color = Color.white;
    private int _score = 0;
    private int _maxBombCount = 1;
    private int _currentBombCount = 1;
    private int _bombPower = 1;
    private float _bombTimer = 2f;
    private int _speedBonus = 1;
    private bool _isDead = false;
    private bool _isInvincible = false;
    private Vector3 _previousPosition = Vector3.zero;

    #endregion

    #region Properties

    public int Id => _id;
    public Color Color => _color;
    public int Score => _score;
    public int Power => _bombPower;
    public  float BombTimer => _bombTimer;
    public int BombCount => _currentBombCount;
    public int MaxBombCount => _maxBombCount;
    public int SpeedBonus => _speedBonus;
    public bool IsDead => _isDead;
    public bool IsInvincible => _isInvincible;

    #endregion

    public void Initialize(int id, Color color)
    {
        _id = id;
        _color = color;
        _spriteRenderer.color = color;
    }

    public virtual void Spawn(Vector3 position)
    {
        _isDead = false;
        _animator.SetBool(ANIMATOR_IS_DEAD_KEY, _isDead);
        _animator.SetTrigger(ANIMATOR_RESPAWN_KEY);

        _rigidbody.simulated = true;

        transform.position = position;

        // Default stats
        UpdatePower(_gameSettings.PlayerBaseBombPower, true);
        UpdateMaxBombCount(_gameSettings.PlayerBaseBombCount, true);
        UpdateSpeedBonus(_gameSettings.PlayerBaseSpeedBonus, true);
        _bombTimer = _gameSettings.PlayerBombBaseTimer;

        SetIsInvicible(true);

        StopCoroutine(SpawnInvicibleTimer(_gameSettings.PlayerSpawnInvincibleTimer));
        StartCoroutine(SpawnInvicibleTimer(_gameSettings.PlayerSpawnInvincibleTimer));

        OnSpawn?.Invoke(this);
    }

    private void SetIsInvicible(bool value)
    {
        _isInvincible = value;
        _animator.SetBool(ANIMATOR_IS_INVINCIBLE_KEY, _isInvincible);
    }

    private IEnumerator SpawnInvicibleTimer(float time)
    {
        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        SetIsInvicible(false);
        yield return null;
    }

    public void AddBomb()
    {
        if (!_isDead && _currentBombCount > 0)
        {
            OnPlantBomb?.Invoke(this);
            OnBombCountChange?.Invoke(this);
        }
    }

    public void PickUpBonus(EBonusType type)
    {
        switch (type)
        {
            case EBonusType.None:
                Debug.LogError("This bonus has no type.");
                break;
            case EBonusType.Power:
                UpdatePower(1);
                break;
            case EBonusType.Bomb:
                UpdateMaxBombCount(1);
                break;
            case EBonusType.Speed:
                UpdateSpeedBonus(1);
                break;
            case EBonusType.Bad:
                // TODO
                break;
            case EBonusType.Score:
                UpdateScore(1);
                break;
            default:
                Debug.LogError("Unknow bonus type.");
                break;
        }

        OnPickUpBonus?.Invoke(this, type);
    }

    public void UpdateCurrentBombCount(int amount)
    {
        _currentBombCount += amount;
        OnBombCountChange?.Invoke(this);
    }

    public void UpdateMaxBombCount(int amount, bool reset = false)
    {
        if (reset)
        {
            _maxBombCount = amount;
            _currentBombCount = amount;
        }
        else
        {
            _maxBombCount += amount;
            _currentBombCount += amount;
        }

        OnBombCountChange?.Invoke(this);
    }

    public void UpdateSpeedBonus(int amount, bool reset = false)
    {
        if (reset)
            _speedBonus = amount;
        else
            _speedBonus += amount;

        OnSpeedChange?.Invoke(this);
    }

    public void UpdatePower(int amount, bool reset = false)
    {
        if (reset)
            _bombPower = amount;
        else
            _bombPower += amount;

        OnPowerChange?.Invoke(this);
    }

    public void UpdateScore(int value)
    {
        _score += value;
        OnScoreChange?.Invoke(this);
    }

    public void OnBombExplosion(Bomb bomb)
    {
        OnBombExploded?.Invoke(bomb.GetImpactedEntities());
    }

    public void OnKilledPlayer(Player player)
    {
        UpdateScore(1);
        OnKill?.Invoke(this);
    }

    public void OnDestroyedWall(DestructibleWall wall)
    {
        OnWallDestroy?.Invoke(this);
    }

    public void OnDestroyedBonus(Bonus bonus)
    {
        OnBonusDestroy?.Invoke(this, bonus.Type);
    }

    public virtual void Kill(Player killer)
    {
        if (_isDead)
            return;

        _isDead = true;

        // Suicide?
        if (killer != null)
        {
            if (killer.Id == Id)
            {
                //Debug.Log($"Player just suicide himself...");
                UpdateScore(-2);
            }
            else
            {
                Debug.Log($"Player killed by {killer.Id}");
            }
        }

        SoundManager.Instance.PlaySound("PlayerDeath");

        _animator.SetBool("IsDead", _isDead);

        OnDeath?.Invoke(this);
    }

    private void Update()
    {
        if (transform.position != _previousPosition)
            OnMove?.Invoke(this);

        _previousPosition = transform.position;
    }

    public void OnDeathAnimationFinish()
    {
        _rigidbody.simulated = false;
        OnDestroyed?.Invoke(this);
    }
}
