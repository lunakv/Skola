#ifndef CUDA_POTENTIAL_IMPLEMENTATION_KERNELS_H
#define CUDA_POTENTIAL_IMPLEMENTATION_KERNELS_H

#include <cuda_runtime.h>
#include <stdexcept>
#include <sstream>
#include <cstdint>

#include "internal/data.hpp"
 

/**
 * A stream exception that is base for all runtime errors.
 */
class CudaError : public std::exception
{
protected:
	std::string mMessage;	///< Internal buffer where the message is kept.
	cudaError_t mStatus;

public:
	CudaError(cudaError_t status = cudaSuccess) : std::exception(), mStatus(status) {}
	CudaError(const char *msg, cudaError_t status = cudaSuccess) : std::exception(), mMessage(msg), mStatus(status) {}
	CudaError(const std::string &msg, cudaError_t status = cudaSuccess) : std::exception(), mMessage(msg), mStatus(status) {}
	virtual ~CudaError() throw() {}

	virtual const char* what() const throw()
	{
		return mMessage.c_str();
	}

	// Overloading << operator that uses stringstream to append data to mMessage.
	template<typename T>
	CudaError& operator<<(const T &data)
	{
		std::stringstream stream;
		stream << mMessage << data;
		mMessage = stream.str();
		return *this;
	}
};


/**
 * CUDA error code check. This is internal function used by CUCH macro.
 */
inline void _cuda_check(cudaError_t status, int line, const char *srcFile, const char *errMsg = NULL)
{
	if (status != cudaSuccess) {
		throw (CudaError(status) << "CUDA Error (" << status << "): " << cudaGetErrorString(status) << "\n"
			<< "at " << srcFile << "[" << line << "]: " << errMsg);
	}
}

/**
 * Macro wrapper for CUDA calls checking.
 */
#define CUCH(status) _cuda_check(status, __LINE__, __FILE__, #status)



/*
 * Kernel wrapper declarations.
 */

typedef std::uint32_t index_t;
typedef double real_t;
typedef Point<double> point_t;
typedef ModelParameters<double> param_t;

void run_calculate_positions(
	index_t* edges,
	index_t* pointStarts,
	point_t* points,
	point_t* pointsNext,
	index_t pointCount,
	point_t* velocities,
	param_t params
);

#endif
