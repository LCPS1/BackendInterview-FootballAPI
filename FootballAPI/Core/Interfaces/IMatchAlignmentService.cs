using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootballAPI.Core.Interfaces
{
    /// <summary>
    /// Service for checking match alignments
    /// </summary>
    public interface IMatchAlignmentService
    {
        Task<bool> CheckMatchAlignmentAsync(int matchId);
        Task<int> NotifyIncorrectAlignmentsAsync();   
    }
}