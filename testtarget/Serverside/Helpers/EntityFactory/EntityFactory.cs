/*
 * @bot-written
 * 
 * WARNING AND NOTICE
 * Any access, download, storage, and/or use of this source code is subject to the terms and conditions of the
 * Full Software Licence as accepted by you before being granted access to this source code and other materials,
 * the terms of which can be accessed on the Codebots website at https://codebots.com/full-software-licence. Any
 * commercial use in contravention of the terms of the Full Software Licence may be pursued by Codebots through
 * licence termination and further legal action, and be required to indemnify Codebots for any loss or damage,
 * including interest and costs. You are deemed to have accepted the terms of the Full Software Licence on any
 * access, download, storage, and/or use of this source code.
 * 
 * BOT WARNING
 * This file is bot-written.
 * Any changes out side of "protected regions" will be lost next time the bot makes any changes.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using My2000thapp.Models;
using My2000thapp.Validators;

namespace ServersideTests.Helpers.EntityFactory
{
	/// <summary>
	/// A factory for creating modeled entities to provide to the tests. This factory can create attributes and
	/// recursively create required references as well.
	/// </summary>
	/// <typeparam name="T">The type of entity to create</typeparam>
	public class EntityFactory<T>
		where T : class, IAbstractModel, new()
	{
		private readonly Dictionary<Type, IAbstractModel> _frozenEntities = new Dictionary<Type, IAbstractModel>();
		private readonly Dictionary<Type, List<(string, object)>> _frozenAttributes = new Dictionary<Type, List<(string, object)>>();
		private readonly int? _totalEntities;
		private bool _trackEntities;
		private bool _useAttributes;
		private bool _useReferences;
		private bool _disableIdGeneration = false;
		private bool _disableCreatedGeneration = false;
		private bool _disableModifiedGeneration = false;
		private Guid? _ownerId;

		/// <summary>
		/// If trackEntities flag in the constructor is set to
		/// </summary>
		public EntityEnumerable<T> EntityEnumerable { get; } = new EntityEnumerable<T>();

		/// <summary>
		/// Creates an entity factory to create modelled entities
		/// </summary>
		/// <param name="totalEntities">
		/// The total number of entities to create. If this value is null then it will create an infinite stream.
		/// </param>
		/// <exception cref="ArgumentException">
		/// If the totalEntities value is less than 1
		/// </exception>
		public EntityFactory(int? totalEntities = 1)
		{
			if (totalEntities <= 0)
			{
				throw new ArgumentException("Total entities cannot be less than one");
			}
			_totalEntities = totalEntities;
		}

		/// <summary>
		/// Should attributes be created by the factory
		/// </summary>
		/// <param name="enabled">Weather to disable or enable attribute generation. Defaults to true</param>
		/// <returns>This entity factory</returns>
		public EntityFactory<T> UseAttributes(bool enabled = true)
		{
			_useAttributes = enabled;
			return this;
		}

		/// <summary>
		/// Should references be created by the factory
		/// </summary>
		/// <param name="enabled">Weather to disable or enable reference generation. Defaults to true</param>
		/// <returns>This entity factory</returns>
		public EntityFactory<T> UseReferences(bool enabled = true)
		{
			_useReferences = enabled;
			return this;
		}

		/// <summary>
		/// Should an owner id be assigned to each created entity
		/// </summary>
		/// <param name="ownerId">The owner id to assign. Set this to null to disable ownership generation</param>
		/// <returns>This entity factory</returns>
		public EntityFactory<T> UseOwner(Guid? ownerId)
		{
			_ownerId = ownerId;
			return this;
		}

		/// <summary>
		/// Disable generation for the Id attribute
		/// </summary>
		/// <param name="disable">Should this be disabled. Defaults to true.</param>
		/// <returns>This entity factory</returns>
		public EntityFactory<T> DisableIdGeneration(bool disable = true)
		{
			_disableIdGeneration = disable;
			return this;
		}

		/// <summary>
		/// Disable generation for the Created attribute
		/// </summary>
		/// <param name="disable">Should this be disabled. Defaults to true.</param>
		/// <returns>This entity factory</returns>
		public EntityFactory<T> DisableCreatedGeneration(bool disable = true)
		{
			_disableCreatedGeneration = disable;
			return this;
		}

		/// <summary>
		/// Disable generation for the Modified attribute
		/// </summary>
		/// <param name="disable">Should this be disabled. Defaults to true.</param>
		/// <returns>This entity factory</returns>
		public EntityFactory<T> DisableModifiedGeneration(bool disable = true)
		{
			_disableModifiedGeneration = disable;
			return this;
		}

		/// <summary>
		/// Should all entities created by this factory be tracked
		/// </summary>
		/// <param name="enabled">Weather to enable or disable entity tracking. Defaults to true</param>
		/// <returns>This entity factory</returns>
		public EntityFactory<T> TrackEntities(bool enabled = true)
		{
			_trackEntities = enabled;
			return this;
		}

		/// <summary>
		/// Freezes the use of an entity to generate when creating references.
		/// This means that the factory will always use this entity for given type in generation.
		/// This will not work for the base list of entities.
		/// </summary>
		/// <param name="entity">The entity to use</param>
		/// <typeparam name="TE">The type of the entity to use</typeparam>
		/// <returns>This entity factory</returns>
		public EntityFactory<T> Freeze<TE>(TE entity)
			where TE : IAbstractModel
		{
			return Freeze(typeof(TE), entity);
		}

		/// <summary>
		/// Freezes the use of an entity to generate when creating references.
		/// This means that the factory will always use this entity for given type in generation.
		/// This will not work for the base list of entities.
		/// </summary>
		/// <param name="type">The type of the entity to use</param>
		/// <param name="entity">The entity to use</param>
		/// <returns>This entity factory</returns>
		public EntityFactory<T> Freeze(Type type, IAbstractModel entity)
		{
			_frozenEntities.Add(type, entity);
			return this;
		}

		/// <summary>
		/// Freezes the use of an attribute on an entity. This means that the factory will always use an attribute of
		/// this value when generating the entity.
		/// </summary>
		/// <param name="attributeName">The name of the attribute to freeze</param>
		/// <param name="value">The value of the attribute to freeze</param>
		/// <typeparam name="TE">The type of the entity to freeze the attribute on</typeparam>
		/// <returns>This entity factory</returns>
		/// <remarks>
		/// Note that the value param is typed as object and it is therefore the developer responsibility to ensure that
		/// it is of the correct type.
		/// </remarks>
		public EntityFactory<T> FreezeAttribute<TE>(string attributeName, object value)
		{
			return FreezeAttribute(typeof(TE), attributeName, value);
		}

		/// <summary>
		/// Freezes the use of an attribute on an entity. This means that the factory will always use an attribute of
		/// this value when generating the entity.
		/// </summary>
		/// <param name="type">The type of the entity to freeze the attribute on</param>
		/// <param name="attributeName">The name of the attribute to freeze</param>
		/// <param name="value">The value of the attribute to freeze</param>
		/// <returns>This entity factory</returns>
		/// <remarks>
		/// Note that the value param is typed as object and it is therefore the developer responsibility to ensure that
		/// it is of the correct type.
		/// </remarks>
		public EntityFactory<T> FreezeAttribute(Type type, string attributeName, object value)
		{
			if (_frozenAttributes.TryGetValue(type, out var frozenAttrs))
			{
				frozenAttrs.Add((attributeName, value));
			}
			else
			{
				_frozenAttributes[type] = new List<(string, object)>
				{
					(attributeName, value)
				};
			}

			return this;
		}

		/// <summary>
		/// Creates a stream of entities.
		/// </summary>
		/// <remarks>
		/// The returned IEnumerable can only be looped over once. If multiple iteration is required then .ToList needs
		/// to be called.
		/// </remarks>
		/// <remarks>
		/// If totalEntites in the constructor was set to null then this will generate an infinite stream of entities.
		/// In this case the number of entities taken from this IEnumerable will need to be limited by a call to .Take.
		/// </remarks>
		/// <returns>The entities described by the factory configuration</returns>
		public IEnumerable<T> Generate()
		{
			if (_totalEntities.HasValue)
			{
				for (var i = 0; i < _totalEntities.Value; i++)
				{
					yield return GenerateEntity();
				}
			}
			else
			{
				while (true)
				{
					yield return GenerateEntity();
				}
			}
		}

		/// <summary>
		/// Generates the individual entities
		/// </summary>
		/// <returns>A new entity</returns>
		protected T GenerateEntity()
		{
			var entity = new T();

			if (_trackEntities)
			{
				EntityEnumerable.AllEntities.Add(entity);
			}

			if (_useAttributes)
			{
				AddAttribute(entity, DateTime.Now, DateTime.Now);
			}

			if (_ownerId.HasValue)
			{
				AddOwnerToModel(entity);
			}

			if (_useReferences)
			{
				CreateAndAddReferences(entity);
			}

			return entity;
		}

		/// <summary>
		/// Adds attributes to an entity
		/// </summary>
		/// <param name="entity">The entity to add attributes to</param>
		/// <param name="created">The created date to add</param>
		/// <param name="modified">The modified date to add</param>
		/// <param name="basePropertiesOnly">Should only common model properties be added</param>
		protected void AddAttribute(
			IAbstractModel entity,
			DateTime? created = null,
			DateTime? modified = null,
			bool basePropertiesOnly = false)
		{
			if (!_disableIdGeneration)
			{
				entity.Id = Guid.NewGuid();
			}

			if (!_disableCreatedGeneration)
			{
				entity.Created = created ?? DateTime.Now;
			}

			if (!_disableModifiedGeneration)
			{
				entity.Modified = modified ?? DateTime.Now;
			}

			var fixture = new Fixture();
			var context = new SpecimenContext(fixture);

			if (basePropertiesOnly)
			{
				return;
			}

			foreach (var attr in EntityFactoryReflectionCache.GetAllAttributes(entity.GetType()))
			{
				var valueSet = false;
				if (_frozenAttributes.TryGetValue(entity.GetType(), out var frozenAttrs))
				{
					var frozenValue = frozenAttrs.FirstOrDefault(x => x.Item1 == attr.Name);
					if (frozenValue != default)
					{
						attr.SetValue(entity, frozenValue.Item2);
						valueSet = true;
					}
				}

				if (!valueSet)
				{
					var customAttributes = attr.GetCustomAttributes().ToList();
					if (customAttributes.Contains(new EmailAttribute()))
					{
						attr.SetValue(entity, TestDataLib.DataUtils.RandEmail());
					}
					else
					{
						attr.SetValue(entity, fixture.Create(attr.PropertyType, context));
					}
				}
			}
		}

		/// <summary>
		/// Adds an owner to a model
		/// </summary>
		/// <param name="entity">The entity to add the owner id to</param>
		protected void AddOwnerToModel(IAbstractModel entity)
		{
			if (entity is IOwnerAbstractModel ownerEntity && _ownerId.HasValue)
			{
				ownerEntity.Owner = _ownerId.Value;
			}
		}

		/// <summary>
		/// Creates an adds any required non collection references to an entity.
		/// </summary>
		/// <param name="entity">The entity to add references to</param>
		/// <param name="visited">
		/// A list of already visited entities. This is used for recursive reference generation and not needed for the
		/// initial call of the function.
		/// </param>
		protected void CreateAndAddReferences(
			IAbstractModel entity,
			List<(Type, string)> visited = null)
		{
			var references = EntityFactoryReflectionCache.GetRequiredReferences(entity.GetType());
			var entityType = entity.GetType();

			if (visited == null)
			{
				visited = new List<(Type, string)>();
			}

			foreach (var reference in references)
			{
				// Detect any loops in the relation list and break if any are found
				if (visited.Contains((entityType, reference.Name)))
				{
					continue;
				}

				if (!_frozenEntities.TryGetValue(reference.Type, out var referenceEntity))
				{
					// Create foreign references and assign them attributes
					referenceEntity = (IAbstractModel)Activator.CreateInstance(reference.Type);
					if (_useAttributes)
					{
						AddAttribute(referenceEntity);
					}

					if (_ownerId.HasValue)
					{
						AddOwnerToModel(referenceEntity);
					}

					// Add the reference to the entity
					EntityFactoryReflectionCache.GetAttribute(entityType, reference.Name)
						.SetValue(entity, referenceEntity);

					// Try to add the reference id to the entity
					try
					{
						EntityFactoryReflectionCache.GetAttribute(entityType, reference.Name + "Id")
							.SetValue(entity, referenceEntity.Id);
					}
					catch
					{
						// Ignore if the Id cannot be set
					}
				}

				if (_trackEntities)
				{
					EntityEnumerable.AllEntities.Add(referenceEntity);
				}

				visited.Add((entityType, reference.Name));

				// Recursively create references for those references
				CreateAndAddReferences(referenceEntity, visited);
			}
		}
	}
}