#include "kernels.h"

__global__ void calculate_positions(
	index_t* edges,
	index_t* pointStarts,
	point_t* points,
	point_t* pointsNext,
	index_t pointCount,
	point_t* velocities,
	param_t params
) {
	index_t idx = blockIdx.x * blockDim.x + threadIdx.x;
	if (idx >= pointCount) return; // in case the point count isn't divisible by blockSize

	point_t thisPoint = points[idx];
	real_t fX = (real_t)0.0;
	real_t fY = (real_t)0.0;

	// calculate repelling forces
	for (index_t i = 0; i < pointCount; ++i) {
		if (i == idx) continue;

		const point_t &other = points[i];
		real_t dx = thisPoint.x - other.x;
		real_t dy = thisPoint.y - other.y;
		real_t r_2 = dx*dx + dy*dy;
		r_2 = max(r_2, 0.0001);
		real_t f = params.vertexRepulsion / r_2;
		f /= sqrt(r_2);
		fX += dx * f;
		fY += dy * f;
	}

	// calculate attracting forces
	index_t neighborsStart = pointStarts[idx];
	index_t neighborsEnd = pointStarts[idx+1];
	for (index_t i = neighborsStart; i < neighborsEnd; ++i) {
		const point_t &other = points[edges[i++]];
		index_t length = edges[i];
		real_t dx = thisPoint.x - other.x;
		real_t dy = thisPoint.y - other.y;
		real_t r_2 = dx*dx + dy*dy;
		real_t f = params.edgeCompulsion * sqrt(r_2) / (real_t)length;
		fX -= dx * f;
		fY -= dy * f;
	}

	// update velocity from force
	real_t dvx = fX * params.timeQuantum / params.vertexMass;
	real_t dvy = fY * params.timeQuantum / params.vertexMass;
	point_t &v = velocities[idx];
	v.x += dvx;
	v.y += dvy;
	v.x *= params.slowdown;
	v.y *= params.slowdown;
	
	// update position
	real_t dsx = v.x * params.timeQuantum;
	real_t dsy = v.y * params.timeQuantum;
	point_t next = points[idx];
	next.x += dsx;
	next.y += dsy;
	pointsNext[idx] = next;
}

/* kernel wrapper */
void run_calculate_positions(
	index_t* edges,
	index_t* pointStarts,
	point_t* points,
	point_t* pointsNext,
	index_t pointCount,
	point_t* velocities,
	param_t params
) {
	const index_t blocksize = 128;
	const index_t gridSize = (pointCount + blocksize - 1) / blocksize;
	calculate_positions<<<gridSize, blocksize>>>(
		edges,
		pointStarts,
		points,
		pointsNext,
		pointCount,
		velocities,
		params
	);
}