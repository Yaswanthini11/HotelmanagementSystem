
using HotelBooking.DTOs.Amenities;
using HotelBooking.Models;
using HotelBooking.Repositories.Interfaces;
using HotelBooking.Services.Interfaces;

namespace HotelBooking.Services.Implementations;

public class AmenityService : IAmenityService
{
    private readonly IAmenityRepository _amenityRepository;

    public AmenityService(IAmenityRepository amenityRepository)
    {
        _amenityRepository = amenityRepository;
    }

    public async Task<IEnumerable<AmenityResponseDto>> GetAllAsync()
    {
        var amenities = await _amenityRepository.GetAllAsync();

        return amenities.Select(MapToResponse);
    }

    public async Task<AmenityResponseDto?> GetByIdAsync(int amenityId)
    {
        var amenity = await _amenityRepository.GetByIdAsync(amenityId);

        return amenity == null ? null : MapToResponse(amenity);
    }

    public async Task<AmenityResponseDto> CreateAsync(
        CreateAmenityRequestDto request)
    {
        var amenity = new Amenity
        {
            Name = request.Name,
            Description = request.Description
        };

        await _amenityRepository.AddAsync(amenity);
        await _amenityRepository.SaveChangesAsync();

        return MapToResponse(amenity);
    }

    public async Task<AmenityResponseDto?> UpdateAsync(
        int amenityId,
        UpdateAmenityRequestDto request)
    {
        var amenity = await _amenityRepository.GetByIdAsync(amenityId);

        if (amenity == null)
            return null;

        amenity.Name = request.Name;
        amenity.Description = request.Description;

        await _amenityRepository.UpdateAsync(amenity);
        await _amenityRepository.SaveChangesAsync();

        return MapToResponse(amenity);
    }

    public async Task<bool> DeleteAsync(int amenityId)
    {
        var deleted = await _amenityRepository.DeleteAsync(amenityId);

        if (!deleted)
            return false;

        await _amenityRepository.SaveChangesAsync();

        return true;
    }

    private static AmenityResponseDto MapToResponse(Amenity amenity)
    {
        return new AmenityResponseDto
        {
            AmenityId = amenity.AmenityId,
            Name = amenity.Name,
            Description = amenity.Description
        };
    }
}
