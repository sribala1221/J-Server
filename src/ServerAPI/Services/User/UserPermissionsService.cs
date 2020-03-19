using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.Models;

namespace ServerAPI.Services
{
    public class UserPermissionsService : IUserPermissionsService {
        private readonly AAtims _db;

        public UserPermissionsService(AAtims db) {
            _db = db;
        }

        public UserAccess GetUser(string userName) {
            return  _db.UserAccess.Single(u => u.UserName == userName);
        }

        public UserPermissions BuildFlatPermissions(int userId) {
            Dictionary<int, PermissionVector> userPermissions = new Dictionary<int, PermissionVector>();
            List<int> groups = (from u in _db.UserGroupXref where u.UserId == userId select u.GroupId).ToList();
            List<PermissionAppAoFunctionality> allPermissions = (from p in _db.PermissionAppAoFunctionality
                                 where groups.Contains(p.GroupId) && p.AllowAccess == 1
                                 select p).ToList();
            allPermissions.ForEach(perm => {
                PermissionVector vector = new PermissionVector {
                    Access = true,
                    Add = perm.AllowAdd == 1,
                    Delete = perm.AllowDelete == 1,
                    Edit = perm.AllowEdit == 1,
                    Print = perm.AllowPrint == 1,
                    RDelete = perm.AllowReleasedDelete == 1,
                    REdit = perm.AllowReleasedEdit == 1
                };
                if (userPermissions.ContainsKey(perm.PermissionAppAoFunctionalityId)) {
                    userPermissions[perm.PermissionAppAoFunctionalityId].Add |= vector.Add;
                    userPermissions[perm.PermissionAppAoFunctionalityId].Delete |= vector.Delete;
                    userPermissions[perm.PermissionAppAoFunctionalityId].Edit |= vector.Edit;
                    userPermissions[perm.PermissionAppAoFunctionalityId].Print |= vector.Print;
                    userPermissions[perm.PermissionAppAoFunctionalityId].RDelete |= vector.RDelete;
                    userPermissions[perm.PermissionAppAoFunctionalityId].REdit |= vector.REdit;
                } else {
                    userPermissions.Add(perm.PermissionAppAoFunctionalityId, vector);
                }
            });
            return new UserPermissions {
                UserId = userId,
                permissions = userPermissions
            };
        }

        public UserPermissionsZip BuildZipPermissions(int userId) {
            Dictionary<int, int[]> userPermissions = new Dictionary<int, int[]>();
            List<int> groups = (from u in _db.UserGroupXref where u.UserId == userId select u.GroupId).ToList();
            List<PermissionAppAoFunctionality> allPermissions = (from p in _db.PermissionAppAoFunctionality
                where groups.Contains(p.GroupId) && p.AllowAccess == 1
                select p).ToList();
            allPermissions.ForEach(perm => {
                int[] vector = 
                {
                    1,
                    perm.AllowAdd ?? 0,
                    perm.AllowDelete ?? 0,
                    perm.AllowEdit  ?? 0,
                    perm.AllowPrint  ?? 0,
                    perm.AllowReleasedDelete ?? 0,
                    perm.AllowReleasedEdit  ?? 0
                };
                if (userPermissions.ContainsKey(perm.PermissionAppAoFunctionalityId))
                {
                    for (int i = 0; i < userPermissions[perm.PermissionAppAoFunctionalityId].Length; i++) {
                        userPermissions[perm.PermissionAppAoFunctionalityId][i] = 
                            Math.Max(userPermissions[perm.PermissionAppAoFunctionalityId][i], vector[i]);   
                    }
                }
                else
                {
                    userPermissions.Add(perm.PermissionAppAoFunctionalityId, vector);
                }
            });
            return new UserPermissionsZip {
                UserId = userId,
                shortPermissions = userPermissions
            };
        }

        public IList<int> BuildPermissionNumbers(int userId) {
            List<int> groups = (from u in _db.UserGroupXref where u.UserId == userId select u.GroupId).ToList();
            List<int> allPermissions = (from p in _db.PermissionAppAoFunctionality
                where groups.Contains(p.GroupId) && p.AllowAccess == 1
                select p.AppAoFunctionalityId).Distinct().ToList();
            return allPermissions;
        }

        
    }
}
