using System.Collections;
using System.Collections.Generic;
using System.Data;
using BaiRong.Core;
using BaiRong.Core.Data;
using SiteServer.CMS.Core;
using SiteServer.CMS.Wcm.Model;
using SiteServer.Plugin;

namespace SiteServer.CMS.Wcm.Provider
{
    public class GovInteractPermissionsDao : DataProviderBase
	{
        private const string SqlSelect = "SELECT UserName, NodeID, Permissions FROM wcm_GovInteractPermissions WHERE UserName = @UserName AND NodeID = @NodeID";
        private const string SqlSelectAll = "SELECT UserName, NodeID, Permissions FROM wcm_GovInteractPermissions WHERE UserName = @UserName";
        private const string SqlInsert = "INSERT INTO wcm_GovInteractPermissions (UserName, NodeID, Permissions) VALUES (@UserName, @NodeID, @Permissions)";
        private const string SqlUpdate = "UPDATE wcm_GovInteractPermissions SET Permissions = @Permissions WHERE UserName = @UserName AND NodeID = @NodeID";
        private const string SqlDelete = "DELETE FROM wcm_GovInteractPermissions WHERE UserName = @UserName AND NodeID = @NodeID";

        private const string ParmUsername = "@UserName";
        private const string ParmNodeId = "@NodeID";
        private const string ParmPermissions = "@Permissions";

		public void Insert(int publishmentSystemId, GovInteractPermissionsInfo permissionsInfo) 
		{
            if (!DataProvider.GovInteractChannelDao.IsExists(permissionsInfo.NodeID))
            {
                var channelInfo = new GovInteractChannelInfo(permissionsInfo.NodeID, publishmentSystemId, 0, 0, string.Empty, string.Empty);
                DataProvider.GovInteractChannelDao.Insert(channelInfo);
            }
			var parms = new IDataParameter[]
			{
				GetParameter(ParmUsername, DataType.NVarChar, 50, permissionsInfo.UserName),
				GetParameter(ParmNodeId, DataType.Integer, permissionsInfo.NodeID),
                GetParameter(ParmPermissions, DataType.Text, permissionsInfo.Permissions)
			};

            ExecuteNonQuery(SqlInsert, parms);
		}

		public void Delete(string userName, int nodeId)
		{
			var parms = new IDataParameter[]
			{
				GetParameter(ParmUsername, DataType.NVarChar, 50, userName),
				GetParameter(ParmNodeId, DataType.Integer, nodeId)
			};

			ExecuteNonQuery(SqlDelete, parms);
		}

        public void Update(GovInteractPermissionsInfo permissionsInfo)
        {
            var parms = new IDataParameter[]
			{
                GetParameter(ParmPermissions, DataType.Text, permissionsInfo.Permissions),
                GetParameter(ParmUsername, DataType.NVarChar, 50, permissionsInfo.UserName),
				GetParameter(ParmNodeId, DataType.Integer, permissionsInfo.NodeID)
			};

            ExecuteNonQuery(SqlUpdate, parms);
        }

        public GovInteractPermissionsInfo GetPermissionsInfo(string userName, int nodeId)
		{
            GovInteractPermissionsInfo permissionsInfo = null;

			var parms = new IDataParameter[]
			{
                GetParameter(ParmUsername, DataType.NVarChar, 50, userName),
				GetParameter(ParmNodeId, DataType.Integer, nodeId)
			};

            using (var rdr = ExecuteReader(SqlSelect, parms))
			{
				if (rdr.Read())
				{
				    var i = 0;
                    permissionsInfo = new GovInteractPermissionsInfo(GetString(rdr, i++), GetInt(rdr, i++), GetString(rdr, i));
				}
				rdr.Close();
			}

            return permissionsInfo;
		}

        public ArrayList GetPermissionsInfoArrayList(string userName)
        {
            var arraylist = new ArrayList();

            var parms = new IDataParameter[]
			{
                GetParameter(ParmUsername, DataType.NVarChar, 50, userName)
			};

            using (var rdr = ExecuteReader(SqlSelectAll, parms))
            {
                while (rdr.Read())
                {
                    var i = 0;
                    var permissionsInfo = new GovInteractPermissionsInfo(GetString(rdr, i++), GetInt(rdr, i++), GetString(rdr, i));
                    arraylist.Add(permissionsInfo);
                }
                rdr.Close();
            }

            return arraylist;
        }

        public Dictionary<int, List<string>> GetPermissionSortedList(string userName)
        {
            var sortedlist = new Dictionary<int, List<string>>();

            var permissionsInfoArrayList = GetPermissionsInfoArrayList(userName);
            foreach (GovInteractPermissionsInfo permissionsInfo in permissionsInfoArrayList)
            {
                var list = new List<string>();
               // if (sortedlist[permissionsInfo.NodeID] != null)//此时sortedlist为空，引起'关键字不在字典中'错误导致用户无法登陆
               //{
               //    list = sortedlist[permissionsInfo.NodeID];
               //}

                var permissionArrayList = TranslateUtils.StringCollectionToStringList(permissionsInfo.Permissions);
                foreach (string permission in permissionArrayList)
                {
                    if (!list.Contains(permission)) list.Add(permission);
                }
                sortedlist[permissionsInfo.NodeID] = list;
            }

            return sortedlist;
        }
	}
}
