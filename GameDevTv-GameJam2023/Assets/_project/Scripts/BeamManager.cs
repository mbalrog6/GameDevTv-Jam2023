using System;
using System.Collections.Generic;
using UnityEngine;

namespace MB6
{
    public class MinorPowerEventArg : EventArgs
    {
        public List<Transform> AffectedObjects;

        public MinorPowerEventArg(List<Transform> affectedObjects)
        {
            AffectedObjects = affectedObjects;
        }
    }
    public class Line
    {
        public int Id;
        public Vector3[] Points;

        public Vector3 Point1
        {
            get
            {
                return Points[0];
            }
            set
            {
                Points[0] = value;
            }
        }
        public Vector3 Point2 
        {
            get
            {
                return Points[1];
            }
            set
            {
                Points[1] = value;
            }
        }

        public Line(int id, Vector3 point1, Vector3 point2)
        {
            Id = id;
            Points = new Vector3[2];
            Points[0] = point1;
            Points[1] = point2;
        }
    }
    public class BeamManager : MonoBehaviour
    {
        public Player _player;
        
        private Dictionary<int, LineRenderer> _lineRenders;
        private Stack<Line> _lines;
        private Stack<Line> _inactiveLines;
        private List<Transform> _objectsTracked;

        private bool _minorPowerActive;

        private void Awake()
        {
            _lineRenders = new Dictionary<int, LineRenderer>();
            _lines = new Stack<Line>();
            _inactiveLines = new Stack<Line>();
            _objectsTracked = new List<Transform>();

            var lineRenders = GetComponentsInChildren<LineRenderer>(true);

            for (var i = 0; i < lineRenders.Length; i++)
            {
                _lineRenders[i] = lineRenders[i];
                _inactiveLines.Push(new Line(i, Vector3.zero, Vector3.zero));
            }
        }

        public void Start()
        {
            _player.OnMinorPower += CreateMinorPowerBeams;
            _player.OnMinorPowerTrackedObjectsChanged += UpdateMinorPowerBeams;
        }

        private void UpdateMinorPowerBeams(object sender, MinorPowerEventArg e)
        {
            if (_player.PlayerEnergyType != EnergyType.Light) return;
            
            _objectsTracked = e.AffectedObjects;
            int difference;

            if (_objectsTracked.Count > _lines.Count)
            {
                difference = _objectsTracked.Count - _lines.Count;

                // Check to see if there are enough beams to render the new list if not allocate
                // the rest of the available beams. 
                if (_inactiveLines.Count < difference)
                {
                    difference = _inactiveLines.Count;
                }

                for (int i = 0; i < difference; i++)
                {
                    _lineRenders[_inactiveLines.Peek().Id].enabled = true;
                    _lines.Push(_inactiveLines.Pop());
                }
            }
            else
            {
                difference = _lines.Count - _objectsTracked.Count;
                
                for (int i = 0; i < difference; i++)
                {
                    _lineRenders[_lines.Peek().Id].enabled = false;
                    _inactiveLines.Push(_lines.Pop());
                }
            }
            
        }

        public void Update()
        {
            if (_minorPowerActive && _player.PlayerEnergyType == EnergyType.Light)
            {
                CalculateLines();
            }
        }

        private void CalculateLines()
        {
            var i = 0; 
            foreach (var line in _lines)
            {
                line.Point1 = _player.transform.position;
                line.Point2 = _objectsTracked[i].position;
                _lineRenders[line.Id].SetPositions(line.Points);
                i++;
            }
        }

        private void CreateMinorPowerBeams(object sender, MinorPowerEventArg e)
        {
            if (_player.PlayerEnergyType != EnergyType.Light) return;
            
            _minorPowerActive = !_minorPowerActive;

            if (_minorPowerActive)
            {
                _objectsTracked = e.AffectedObjects;

                foreach (var obj in e.AffectedObjects)
                {
                    if (_inactiveLines.Count < 1) break;
                    
                    var line = _inactiveLines.Pop();
                    line.Point1 = _player.transform.position;
                    line.Point2 = obj.position;
                    
                    _lines.Push(line);
                    _lineRenders[line.Id].enabled = true;
                    _lineRenders[line.Id].SetPositions(line.Points);
                }

                return;
            }
            
            ClearLines();
        }

        public void ClearLines()
        {
            _objectsTracked = null;

            var numberOfLines = _lines.Count;
            for (var i = 0; i < numberOfLines; i++)
            {
                _lineRenders[_lines.Peek().Id].enabled = false;
                _inactiveLines.Push(_lines.Pop());
            }
        }
    }
}