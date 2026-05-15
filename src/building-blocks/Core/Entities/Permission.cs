namespace Educore.Core.Entities;

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Resource { get; set; } = "";
    public string Action { get; set; } = "";
    public string Name { get; set; } = "";
}

public class PermissionGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public Guid OrganizationId { get; set; }
}

public class UserPermission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
    public bool Granted { get; set; } = true;
}

public class UserGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
}

public class GroupPermission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupId { get; set; }
    public Guid PermissionId { get; set; }
}
