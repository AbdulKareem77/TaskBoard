export interface Project {
  id: string;
  name: string;
  description: string | null;
  ownerName: string;
  memberCount: number;
  taskCount: number;
  isArchived: boolean;
  dateCreated: string;
}

export interface ProjectMember {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

export interface ProjectDetail extends Project {
  ownerId: string;
  members: ProjectMember[];
}
