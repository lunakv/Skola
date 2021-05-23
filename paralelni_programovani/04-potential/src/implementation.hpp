#ifndef CUDA_POTENTIAL_IMPLEMENTATION_HPP
#define CUDA_POTENTIAL_IMPLEMENTATION_HPP

#include "kernels.h"

#include <interface.hpp>
#include <data.hpp>

#include <cuda_runtime.h>


/*
 * Final implementation of the tested program.
 */
template<typename F = float, typename IDX_T = std::uint32_t, typename LEN_T = std::uint32_t>
class ProgramPotential : public IProgramPotential<F, IDX_T, LEN_T>
{
public:
	typedef F coord_t;		// Type of point coordinates.
	typedef coord_t real_t;	// Type of additional float parameters.
	typedef IDX_T index_t;
	typedef LEN_T length_t;
	typedef Point<coord_t> point_t;
	typedef Edge<index_t> edge_t;
	typedef ModelParameters<F> param_t;

private:
	index_t* cu_edges;
	index_t* cu_pointStarts;

	point_t* cu_points;
	point_t* cu_pointsNext;
	point_t* cu_velocities;

	index_t currentIteration;
	bool pointsInitialized{};
	index_t pointCount;


public:
	virtual void initialize(index_t points, const std::vector<edge_t>& edges, const std::vector<length_t> &lengths, index_t iterations)
	{
		pointCount = points;
		std::vector<index_t> transformedEdges;
		std::vector<index_t> pointStarts;
		// transformedEdges is a flattened incidence list laid out as follows: [ point, length, point, length, ...]
		// where point is a neighbor on an edge and length is the length of that edge
		// pointStarts stores for each point the start of a section on transformedEdges that belongs to it.

		/* Transforming the edges array */
		// build an incidence list
		std::vector<std::vector<index_t>> edgeMap(points);
		for (size_t i = 0; i < edges.size(); ++i) {
			const edge_t &e = edges[i];
			edgeMap[e.p1].push_back(e.p2);
			edgeMap[e.p2].push_back(e.p1);
			edgeMap[e.p1].push_back(lengths[i]);
			edgeMap[e.p2].push_back(lengths[i]);
		}

		// flatten the list
		for (size_t i = 0; i < points; ++i) {
			pointStarts.push_back(transformedEdges.size());
			for (size_t j = 0; j < edgeMap[i].size(); ++j) {
				transformedEdges.push_back(edgeMap[i][j]);
			}
		}
		pointStarts.push_back(transformedEdges.size());

		std::vector<point_t> zeroes(points); // for initial velocities

		CUCH(cudaSetDevice(0));
		CUCH(cudaMalloc(&cu_edges, transformedEdges.size() * sizeof(index_t)));
		CUCH(cudaMalloc(&cu_pointStarts, pointStarts.size() * sizeof(index_t)));
		CUCH(cudaMalloc(&cu_points, points * sizeof(point_t)));
		CUCH(cudaMalloc(&cu_pointsNext, points * sizeof(point_t)));
		CUCH(cudaMalloc(&cu_velocities, points * sizeof(point_t)));

		CUCH(cudaMemcpy(cu_edges, &transformedEdges[0], transformedEdges.size() * sizeof(index_t), cudaMemcpyHostToDevice));
		CUCH(cudaMemcpy(cu_pointStarts, &pointStarts[0], pointStarts.size() * sizeof(index_t), cudaMemcpyHostToDevice));
		CUCH(cudaMemcpy(cu_velocities, &zeroes[0], points * sizeof(point_t), cudaMemcpyHostToDevice));
	}


	virtual void iteration(std::vector<point_t> &points)
	{
		/*
		 * Perform one iteration of the simulation and update positions of the points.
		 */
		if (!pointsInitialized) {
			// only copy the points array from host to device on first call
			CUCH(cudaMemcpy(cu_points, &points[0], pointCount * sizeof(point_t), cudaMemcpyHostToDevice));
			pointsInitialized = true;
		}

		run_calculate_positions(
			cu_edges,
			cu_pointStarts,
			cu_points,
			cu_pointsNext,
			pointCount,
			cu_velocities,
			this->mParams
		);
		CUCH(cudaGetLastError());

		CUCH(cudaMemcpy(&points[0], cu_pointsNext, pointCount * sizeof(point_t), cudaMemcpyDeviceToHost));
		std::swap(cu_points, cu_pointsNext);
	}


	virtual void getVelocities(std::vector<point_t> &velocities)
	{
		/*
		 * Retrieve the velocities buffer from the GPU.
		 * This operation is for verification only and it does not have to be efficient.
		 */
		velocities.resize(pointCount);
		CUCH(cudaMemcpy(&velocities[0], cu_velocities, pointCount * sizeof(point_t), cudaMemcpyDeviceToHost));
	}
};


#endif
