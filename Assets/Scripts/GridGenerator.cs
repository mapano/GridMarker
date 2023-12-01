using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class GridGenerator : MonoBehaviour
{

    [SerializeField]
    private SpawnerController spawnerController;
    [SerializeField]
    private TilePoolManager tilePoolManager;
    [SerializeField] private GridVisualizer gridVisualizer;
    private Grid _grid;
    [SerializeField] private UIController uiController;
    private Camera _mainCamera;
    [SerializeField] 
    private float minSize = 5f;
    [SerializeField]
    private float maxSize = 100;

    
    private Vector3 _origin;
    private bool _isPanning;
    
    void Start()
    {
        Application.targetFrameRate = 60;
        _mainCamera = Camera.main;
        _mainCamera.orthographicSize = minSize;
        uiController.Init(ClearBoard, spawnerController.StartSpawning, spawnerController.StopSpawning, ChangeCameraZoom );
        _grid = new Grid();
        _grid.Load("gridSize.txt");
        gridVisualizer.Init(_grid);
        tilePoolManager.Init();
        spawnerController.Init(_grid, tilePoolManager);
        gridVisualizer.DisplayGrid();
    }

    private void Update()
    {
        spawnerController?.OnUpdate();
        HandleCameraPanning();
    }

    private void ClearBoard()
    {
        _grid.ClearNeighbours();
        _grid.ResetSpiral();
        gridVisualizer.ClearTilesInTilemap();
    }
    
    void ChangeCameraZoom(float zoomValue)
    {
        float newFOV = Mathf.Lerp(minSize, maxSize, zoomValue);
        _mainCamera.orthographicSize = newFOV; 
    }
    
    private void HandleCameraPanning()
    {
        if(spawnerController.IsDragging)
            return;
        
        if (Input.GetMouseButtonDown(0)) 
        {
            _origin = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            _isPanning = true;
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            _isPanning = false;
        }

        if (_isPanning)
        {
            Vector3 difference = _origin - _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            _mainCamera.transform.position += difference;
        }
    }
}