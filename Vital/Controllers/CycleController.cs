﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Dto.Cycle;
using Models.Identity;
using Models.Pagination;
using Vital.Core.Context;
using Vital.Core.Services.Interfaces;
using Vital.Extension.Mapping;
using Vital.Models.Exception;

namespace Vital.Controllers;

/// <summary>
/// Controller responsible for accessing Cycle data.
/// </summary>
[Authorize]
public class CycleController : BaseController
{
    private readonly ICycleService _cycleService;
    private readonly IMapper _mapper;
    private readonly CurrentContext _currentContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public CycleController(ICycleService cycleService, IMapper mapper, CurrentContext currentContext, UserManager<ApplicationUser> userManager)
    {
        _cycleService = cycleService;
        _mapper = mapper;
        _currentContext = currentContext;
        _userManager = userManager;
    }

    /// <summary>
    /// Retrieves a paginated list of Cycle objects.
    /// </summary>
    /// <param name="paginator">An object of type Paginator which contains parameters for pagination.</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<CycleDto>))]
    public async Task<IActionResult> GetAll([FromQuery] Paginator paginator)
    {
        var list = await _cycleService.Get(paginator);

        return Ok(_mapper.MapPaginatedList<Cycle, CycleDto>(list));
    }

    /// <summary>
    /// Retrieves a Cycle object by its ID.
    /// </summary>
    /// <param name="id">The unique Guid identifier of the Cycle object.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CycleDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var cycle = await _cycleService.GetById(id);
        if (cycle is null)
        {
            throw new NotFoundException();
        }

        return Ok(cycle);
    }

    /// <summary>
    /// Creates a new Cycle object with the specified details.
    /// </summary>
    /// <param name="dto">A CreateCycleDto object containing the details for the new Cycle object.</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CycleDto))]
    public async Task<IActionResult> Create()
    {
        // End the user's current cycle, we can use ! here because we know the user is authenticated and the UserId is set.
        var user = await _userManager.FindByIdAsync(_currentContext.UserId!.Value.ToString());
        var currentCycle = await _cycleService.GetCurrentCycle(user!.Id);
        if (currentCycle != null)
        {
            currentCycle.EndDate = DateTimeOffset.Now;
            await _cycleService.Update(currentCycle.Id, new UpdateCycleDto()
            {
                StartDate = currentCycle.StartDate,
                EndDate = DateTimeOffset.Now
            });
        }
        
        var cycle = await _cycleService.Create();
        user!.CurrentCycleId = cycle.Id;
        await _userManager.UpdateAsync(user);
        return Created("", cycle);
    }

    /// <summary>
    /// Updates an existing Cycle object with new values.
    /// </summary>
    /// <param name="id">The unique identifier of the Cycle object to be updated.</param>
    /// <param name="dto">A UpdateCycleDto object containing the updated values.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CycleDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCycleDto dto)
    {
        var cycle = await _cycleService.Update(id, dto);

        return Ok(cycle);
    }

    /// <summary>
    /// Retrieves a list of predicted period days for a Cycle object.
    /// </summary>
    /// <param name="cycleId">The ID of the predicted cycle.</param>
    [HttpGet("predicted-period")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DateTimeOffset>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPredictedPeriod()
    {
        var userId = _currentContext.UserId!.Value;
        var predictedPeriodDays = await _cycleService.GetPredictedPeriod(userId);
        return Ok(predictedPeriodDays);
    }
    
    [HttpGet("current-cycle")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Cycle))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentCycle()
    {
        var userId = _currentContext.UserId!.Value;
        var cycle = await _cycleService.GetCurrentCycle(userId);
        return Ok(cycle);
    }
}
