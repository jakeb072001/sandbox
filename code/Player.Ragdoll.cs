﻿using Sandbox;

partial class SandboxPlayer
{
	static readonly EntityLimit RagdollLimit = new() { MaxTotal = 10 };

	[ClientRpc]
	private void BecomeRagdollOnClient( Vector3 velocity, DamageFlags damageFlags, Vector3 forcePos, Vector3 force )
	{
		var ent = new ModelEntity();
		ent.WorldPos = WorldPos;
		ent.WorldRot = WorldRot;
		ent.WorldScale = WorldScale;
		ent.MoveType = MoveType.Physics;
		ent.UsePhysicsCollision = true;
		ent.EnableAllCollisions = true;
		ent.CollisionGroup = CollisionGroup.Debris;
		ent.SetModel( GetModelName() );
		ent.CopyBonesFrom( this );
		ent.TakeDecalsFrom( this );
		ent.EnableHitboxes = true;
		ent.EnableAllCollisions = true;
		ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ent.CopyBodyGroups( this );
		ent.CopyMaterialGroup( this );
		ent.RenderColorAndAlpha = RenderColorAndAlpha;
		ent.PhysicsGroup.Velocity = velocity;
		ent.EnableDrawing = false;

		ent.SetInteractsAs( CollisionLayer.Debris );
		ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

		foreach ( var child in Children )
		{
			if ( child is ModelEntity e )
			{
				var model = e.GetModelName();
				if ( model != null && !model.Contains( "clothes" ) )
					continue;

				var clothing = new ModelEntity();
				clothing.SetModel( model );
				clothing.SetParent( ent, true );
				clothing.RenderColorAndAlpha = e.RenderColorAndAlpha;
				clothing.EnableDrawing = false;
			}
		}

		if ( damageFlags.HasFlag( DamageFlags.Blast ) )
		{
			if ( ent.PhysicsGroup != null )
			{
				ent.PhysicsGroup.AddVelocity( (WorldPos - (forcePos + Vector3.Down * 100.0f)).Normal * (force.Length * 0.2f) );
				var angularDir = (Rotation.FromYaw( 90 ) * force.WithZ( 0 ).Normal).Normal;
				ent.PhysicsGroup.AddAngularVelocity( angularDir * (force.Length * 0.02f) );
			}
		}

		Corpse = ent;

		RagdollLimit.Watch( ent );
	}
}
