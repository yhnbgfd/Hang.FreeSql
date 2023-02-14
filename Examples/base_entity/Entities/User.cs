﻿using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class UserGroup : BaseEntity<UserGroup, int>
{
    /// <summary>
    /// 组名
    /// </summary>
    public string GroupName { get; set; }

    public List<User1> User1s { get; set; }
}

public class Role : BaseEntity<Role, string>
{
    public List<User1> User1s { get; set; }
}
public class RoleUser1 : BaseEntity<RoleUser1>
{
    public string RoleId { get; set; }
    public Guid User1Id { get; set; }

    public Role Role { get; set; }
    public User1 User1 { get; set; }
}

public class User1 : BaseEntity<User1, Guid>
{
    public int GroupId { get; set; }
    public UserGroup Group { get; set; }

    public virtual List<Role> Roles { get; set; }

    /// <summary>
    /// 登陆名
    /// </summary>
    [MaxLength(32)]
    public string Username { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [MaxLength(64)]
    public string Nickname { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    [MaxLength(1024)]
    public string Avatar { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(4000)]
    public string Description { get; set; }
}

public class IdentityTable
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int id { get; set; }

    public string name { get; set; }
}