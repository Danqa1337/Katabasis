using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class RendererComponent : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private bool _isInInvis;
    private bool _isPetrified;
    private bool _isDecayed;
    private bool _isHidenOnSelection;
    private bool _altSpriteDrown;
    private int _spriteIndex;

    private ObjectSpritesCollection _spritesCollection;
    public bool isMaterialModified => _isInInvis || _isPetrified || _isDecayed || _isHidenOnSelection;
    public Material material => _spriteRenderer.material;
    public ObjectSpritesCollection ObjectSpritesCollection => _spritesCollection;
    public List<Sprite> Sprites => ObjectSpritesCollection.sprites;
    public List<Sprite> AltSprites => ObjectSpritesCollection.alternativeSprites;

    public Color color
    {
        get => _spriteRenderer.color;
        set => _spriteRenderer.color = value;
    }

    public float2 SpriteCenterOffset => ObjectSpritesCollection.spriteCenterOffset;

    public Sprite Sprite => _spriteRenderer.sprite;

    public int sortingOrder
    {
        get
        {
            return _spriteRenderer.sortingOrder;
        }
        set
        {
            _spriteRenderer.sortingOrder = value;
        }
    }

    public string spritesSortingLayer
    {
        get
        {
            return _spriteRenderer.sortingLayerName;
        }
        set
        {
            _spriteRenderer.sortingLayerName = value;
        }
    }

    public bool IsInInvis { get => _isInInvis; set => _isInInvis = value; }
    public bool IsPetrified { get => _isPetrified; set => _isPetrified = value; }
    public bool AltSpriteDrown { get => _altSpriteDrown; }
    public int SpriteIndex { get => _spriteIndex; }

    public void DrawCollection(ObjectSpritesCollection spritesCollection, int spriteIndex, bool drawAltSprite)
    {
        _spritesCollection = spritesCollection;
        _spriteIndex = spriteIndex;
        _altSpriteDrown = drawAltSprite;
        var sprite = _spritesCollection.sprites[0];
        if (!_spritesCollection.hasSeamlessTexture)
        {
            if (_spriteIndex == -1)
            {
                _spriteIndex = UnityEngine.Random.Range(0, _spritesCollection.sprites.Count);
            }

            if (_spritesCollection.sprites.Count <= SpriteIndex) throw new System.ArgumentOutOfRangeException("rndSprite num " + _spriteIndex + " is out of range " + _spritesCollection.sprites.Count);

            if (_altSpriteDrown)
            {
                sprite = spritesCollection.alternativeSprites[_spriteIndex];
            }
            else
            {
                sprite = spritesCollection.sprites[_spriteIndex];
            }
        }
        else
        {
            sprite = spritesCollection.sprites[30];
        }
        _spriteRenderer.sprite = sprite;
    }

    public void SwitchToAltSprite()
    {
        _altSpriteDrown = true;
        _spriteRenderer.sprite = AltSprites[_spriteIndex];
    }

    public void SwitchToNormalSprite()
    {
        _altSpriteDrown = false;
        _spriteRenderer.sprite = Sprites[_spriteIndex];
    }

    public void ResetAll()
    {
        Show();
        ResetMaterial();
        SetMaskInnteraction(SpriteMaskInteraction.None);
        transform.localScale = new Vector3(1, 1, 1);
        transform.position = transform.position;
        transform.localRotation = quaternion.Euler(Vector3.zero);
        sortingOrder = 0;
    }

    public void ResetMaterial()
    {
        _isDecayed = false;
        _isInInvis = false;
        _isPetrified = false;
        _spriteRenderer.sharedMaterial = LowLevelSettings.instance.defaultMaterial;
    }

    public void SetMaskInnteraction(SpriteMaskInteraction spriteMaskInteraction)
    {
        _spriteRenderer.maskInteraction = spriteMaskInteraction;
    }

    public async Task Desolve()
    {
        for (float f = 0; f < 1f; f += 0.05f)
        {
            _spriteRenderer.material.SetFloat("Desolve", f);
            await Task.Delay(20);
        }
        _spriteRenderer.material.SetFloat("Desolve", 0);
        gameObject.SetActive(false);
        ResetMaterial();
    }

    private void Fill(Color color)
    {
        _spriteRenderer.material.SetColor("FillColor", color);
    }

    public void Petrify()
    {
        _spriteRenderer.material.SetFloat("Petrified", 1);
        _isPetrified = true;
    }

    public void UnPetrify()
    {
        _spriteRenderer.material.SetFloat("Petrified", 0);

        _isPetrified = false;
        if (!isMaterialModified) ResetMaterial();
    }

    public void OnTileAboveSelected()
    {
        if (Sprite.textureRect.height > 32)
        {
            _isHidenOnSelection = true;
            _spriteRenderer.material.SetFloat("HideWall", 1);
        }
    }

    public void OnTileAboveDeselected()
    {
        if (Sprite.textureRect.height > 32)
        {
            _isHidenOnSelection = false;
            _spriteRenderer.material.SetFloat("HideWall", 0);
            if (!isMaterialModified) ResetMaterial();
        }
    }

    public void BecomeInvisible()
    {
        _isInInvis = true;
        Debug.Log("invis");
        _spriteRenderer.material.SetFloat("invisible", 1);

        _spriteRenderer.gameObject.layer = 8;
    }

    public void BecomeDecayed()
    {
        _isDecayed = true;
        _spriteRenderer.material.SetFloat("Decayed", 1);
    }

    public void Hide()
    {
        _spriteRenderer.gameObject.layer = 6;
    }

    public void Show()
    {
        _spriteRenderer.gameObject.layer = 0;
    }

    public void BecomeVisible()
    {
        _isInInvis = false;
        _spriteRenderer.material.SetFloat("invisible", 0);
        _spriteRenderer.gameObject.layer = 0;
        if (!isMaterialModified) ResetMaterial();
    }

    public async void DrawHitAnimation()
    {
        if (_spriteRenderer.material.GetFloat("invisible") == 0)
        {
            Fill(Color.gray);

            await Task.Delay(150);

            Fill(Color.clear);
            if (!isMaterialModified) ResetMaterial();
        }
    }
}