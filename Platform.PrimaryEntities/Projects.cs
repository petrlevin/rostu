using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.DbEnums;

namespace Platform.PrimaryEntities
{
    /// <summary>
    /// 
    /// </summary>
    public static class Solution
    {
        /// <summary>
        /// Имя проекта
        /// </summary>
        /// <param name="idSolutionProject">Идентификатор проекта</param>
        public static string ProjectName(int idSolutionProject)
        {
            try
            {
                return ProjectName((SolutionProject) idSolutionProject);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Передан неверный идентификатор проекта", "idSolutionProject");
            }
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="solutionProject"></param>
        /// <returns></returns>
        public static string ProjectName(SolutionProject solutionProject)
        {
                return solutionProject.ToString().Replace("_", ".");

        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idSolutionProject"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string EntityClassProjectName(int idSolutionProject)
        {
            try
            {
                return EntityClassProjectName((SolutionProject)idSolutionProject);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Передан неверный идентификатор проекта", "idSolutionProject");
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solutionProject"></param>
        /// <returns></returns>
        public static string EntityClassProjectName(SolutionProject solutionProject)
        {
            if (solutionProject != SolutionProject.Tools_MigrationHelper_Core)
                return ProjectName(solutionProject);
            else
                return ProjectName(SolutionProject.Platform_PrimaryEntities);
        }

    }
}
