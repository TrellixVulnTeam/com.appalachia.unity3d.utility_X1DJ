﻿using System;
using System.Collections.Generic;
using Appalachia.Utility.DataStructures.Common;

// ReSharper disable UnusedParameter.Global

namespace Appalachia.Utility.DataStructures.Graphs
{
    /// <summary>
    ///     This class represents the graph as an adjacency-matrix (two dimensional integer array).
    /// </summary>
    public class DirectedWeightedDenseGraph<T> : DirectedDenseGraph<T>, IWeightedGraph<T>
        where T : IComparable<T>
    {
        #region Constants and Static Readonly

        /// <summary>
        ///     INSTANCE VARIABLES
        /// </summary>
        private const long EMPTY_EDGE_SLOT = 0;

        private const object EMPTY_VERTEX_SLOT = null;

        #endregion

        /// <summary>
        ///     CONSTRUCTOR
        /// </summary>
        public DirectedWeightedDenseGraph(uint capacity = 10)
        {
            _edgesCount = 0;
            _verticesCount = 0;
            _verticesCapacity = (int)capacity;

            _vertices = new List<object>(_verticesCapacity);
            _adjacencyMatrix = new long[_verticesCapacity, _verticesCapacity];
            _adjacencyMatrix.Populate(_verticesCapacity, _verticesCapacity);
        }

        #region Fields and Autoproperties

        // Store edges and their weights as integers.
        // Any edge with a value of zero means it doesn't exist. Otherwise, it exist with a specific weight value.
        // Default value for positive edges is 1.
        protected new long[,] _adjacencyMatrix { get; set; }

        #endregion

        /// <summary>
        ///     An enumerable collection of all weighted directed edges in graph.
        /// </summary>
        public new virtual IEnumerable<WeightedEdge<T>> Edges
        {
            get
            {
                foreach (var vertex in _vertices)
                foreach (var outgoingEdge in OutgoingEdges((T)vertex))
                {
                    yield return outgoingEdge;
                }
            }
        }

        /// <summary>
        ///     Returns true, if graph is weighted; false otherwise.
        /// </summary>
        public override bool IsWeighted => true;

        /// <summary>
        ///     Get all incoming unweighted edges to a vertex.
        /// </summary>
        public new virtual IEnumerable<WeightedEdge<T>> IncomingEdges(T vertex)
        {
            if (!HasVertex(vertex))
            {
                throw new KeyNotFoundException("Vertex doesn't belong to graph.");
            }

            var source = _vertices.IndexOf(vertex);

            for (var adjacent = 0; adjacent < _vertices.Count; ++adjacent)
            {
                if ((_vertices[adjacent] != null) && _doesEdgeExist(adjacent, source))
                {
                    yield return new WeightedEdge<T>(
                        (T)_vertices[adjacent],          // from
                        vertex,                          // to
                        _getEdgeWeight(source, adjacent) // weight
                    );
                }
            } //end-for
        }

        /// <summary>
        ///     Get all outgoing unweighted edges from a vertex.
        /// </summary>
        public new virtual IEnumerable<WeightedEdge<T>> OutgoingEdges(T vertex)
        {
            if (!HasVertex(vertex))
            {
                throw new KeyNotFoundException("Vertex doesn't belong to graph.");
            }

            var source = _vertices.IndexOf(vertex);

            for (var adjacent = 0; adjacent < _vertices.Count; ++adjacent)
            {
                if ((_vertices[adjacent] != null) && _doesEdgeExist(source, adjacent))
                {
                    yield return new WeightedEdge<T>(
                        vertex,                          // from
                        (T)_vertices[adjacent],          // to
                        _getEdgeWeight(source, adjacent) // weight
                    );
                }
            } //end-for
        }

        /// <summary>
        ///     Clear this graph.
        /// </summary>
        public override void Clear()
        {
            _edgesCount = 0;
            _verticesCount = 0;
            _vertices = new List<object>(_verticesCapacity);
            _adjacencyMatrix = new long[_verticesCapacity, _verticesCapacity];
            _adjacencyMatrix.Populate(_verticesCapacity, _verticesCapacity);
        }

        /// <summary>
        ///     Removes edge, if exists, from source to destination.
        /// </summary>
        public override bool RemoveEdge(T source, T destination)
        {
            // Get indices of vertices
            var srcIndex = _vertices.IndexOf(source);
            var dstIndex = _vertices.IndexOf(destination);

            // Check existence of vertices and non-existence of edge
            if ((srcIndex == -1) || (dstIndex == -1))
            {
                return false;
            }

            if (!_doesEdgeExist(srcIndex, dstIndex))
            {
                return false;
            }

            _adjacencyMatrix[srcIndex, dstIndex] = EMPTY_EDGE_SLOT;

            // Increment edges count
            --_edgesCount;

            return true;
        }

        /// <summary>
        ///     Removes the specified vertex from graph.
        /// </summary>
        public override bool RemoveVertex(T vertex)
        {
            // Return if graph is empty
            if (_verticesCount == 0)
            {
                return false;
            }

            // Get index of vertex
            var index = _vertices.IndexOf(vertex);

            // Return if vertex doesn't exists
            if (index == -1)
            {
                return false;
            }

            // Lazy-delete the vertex from graph
            //_vertices.Remove (vertex);
            _vertices[index] = EMPTY_VERTEX_SLOT;

            // Decrement the vertices count
            --_verticesCount;

            // Remove all outgoing and incoming edges to this vertex
            for (var i = 0; i < _verticesCapacity; ++i)
            {
                // Outgoing edge
                if (_doesEdgeExist(index, i))
                {
                    _adjacencyMatrix[index, i] = EMPTY_EDGE_SLOT;

                    // Decrement the edges count
                    --_edgesCount;
                }

                // Incoming edge
                if (_doesEdgeExist(i, index))
                {
                    _adjacencyMatrix[i, index] = EMPTY_EDGE_SLOT;

                    // Decrement the edges count
                    --_edgesCount;
                }
            }

            return true;
        }

        /// <summary>
        ///     Returns a human-readable string of the graph.
        /// </summary>
        public override string ToReadable()
        {
            var output = string.Empty;

            for (var i = 0; i < _vertices.Count; ++i)
            {
                if (_vertices[i] == null)
                {
                    continue;
                }

                var node = (T)_vertices[i];
                var adjacents = string.Empty;

                output = string.Format("{0}\r\n{1}: [", output, node);

                foreach (var adjacentNode in NeighboursMap(node))
                {
                    adjacents = string.Format(
                        "{0}{1}({2}), ",
                        adjacents,
                        adjacentNode.Key,
                        adjacentNode.Value
                    );
                }

                if (adjacents.Length > 0)
                {
                    adjacents = adjacents.TrimEnd(',', ' ');
                }

                output = string.Format("{0}{1}]", output, adjacents);
            }

            return output;
        }

        /// <summary>
        ///     Obsolete. Another AddEdge function is implemented with a weight parameter.
        /// </summary>
        [Obsolete("Use the AddEdge method with the weight parameter.")]
        public new bool AddEdge(T source, T destination)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Helper function. Checks if edge exist in graph.
        /// </summary>
        protected override bool _doesEdgeExist(int source, int destination)
        {
            return _adjacencyMatrix[source, destination] != EMPTY_EDGE_SLOT;
        }

        /// <summary>
        ///     Helper function. Gets the weight of a directed edge.
        /// </summary>
        private long _getEdgeWeight(int source, int destination)
        {
            return _adjacencyMatrix[source, destination];
        }

        #region IWeightedGraph<T> Members

        /// <summary>
        ///     Connects two vertices together with a weight, in the direction: first->second.
        /// </summary>
        public virtual bool AddEdge(T source, T destination, long weight)
        {
            // Return if the weight is equals to the empty edge value
            if (weight == EMPTY_EDGE_SLOT)
            {
                return false;
            }

            // Get indices of vertices
            var srcIndex = _vertices.IndexOf(source);
            var dstIndex = _vertices.IndexOf(destination);

            // Check existence of vertices and non-existence of edge
            if ((srcIndex == -1) || (dstIndex == -1))
            {
                return false;
            }

            if (_doesEdgeExist(srcIndex, dstIndex))
            {
                return false;
            }

            _adjacencyMatrix[srcIndex, dstIndex] = weight;

            // Increment edges count
            ++_edgesCount;

            return true;
        }

        /// <summary>
        ///     Updates the edge weight from source to destination.
        /// </summary>
        public virtual bool UpdateEdgeWeight(T source, T destination, long weight)
        {
            // Return if the weight is equals to the empty edge value
            if (weight == EMPTY_EDGE_SLOT)
            {
                return false;
            }

            // Get indices of vertices
            var srcIndex = _vertices.IndexOf(source);
            var dstIndex = _vertices.IndexOf(destination);

            // Check existence of vertices and non-existence of edge
            if ((srcIndex == -1) || (dstIndex == -1))
            {
                return false;
            }

            if (!_doesEdgeExist(srcIndex, dstIndex))
            {
                return false;
            }

            _adjacencyMatrix[srcIndex, dstIndex] = weight;

            return true;
        }

        /// <summary>
        ///     Get edge object from source to destination.
        /// </summary>
        public virtual WeightedEdge<T> GetEdge(T source, T destination)
        {
            // Get indices of vertices
            var srcIndex = _vertices.IndexOf(source);
            var dstIndex = _vertices.IndexOf(destination);

            // Check the existence of vertices and the directed edge
            if ((srcIndex == -1) || (dstIndex == -1))
            {
                throw new Exception("One of the vertices or both of them doesn't exist.");
            }

            if (!_doesEdgeExist(srcIndex, dstIndex))
            {
                throw new Exception("Edge doesn't exist.");
            }

            return new WeightedEdge<T>(source, destination, _getEdgeWeight(srcIndex, dstIndex));
        }

        /// <summary>
        ///     Returns the edge weight from source to destination.
        /// </summary>
        public virtual long GetEdgeWeight(T source, T destination)
        {
            return GetEdge(source, destination).Weight;
        }

        /// <summary>
        ///     Returns the neighbours of a vertex as a dictionary of nodes-to-weights.
        /// </summary>
        public virtual Dictionary<T, long> NeighboursMap(T vertex)
        {
            if (!HasVertex(vertex))
            {
                return null;
            }

            var neighbors = new Dictionary<T, long>();
            var source = _vertices.IndexOf(vertex);

            // Check existence of vertex
            if (source != -1)
            {
                for (var adjacent = 0; adjacent < _vertices.Count; ++adjacent)
                {
                    if ((_vertices[adjacent] != null) && _doesEdgeExist(source, adjacent))
                    {
                        neighbors.Add((T)_vertices[adjacent], _getEdgeWeight(source, adjacent));
                    }
                }
            }

            return neighbors;
        }

        #endregion
    }
}