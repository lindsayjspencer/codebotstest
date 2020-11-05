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
import * as React from 'react';
import { QueryResult, Query, OperationVariables } from 'react-apollo';
import { DocumentNode } from 'graphql';
import { Model } from 'Models/Model';
import PaginationData, { PaginationQueryOptions } from 'Models/PaginationData';
import {getModelName, getFetchAllQuery, getFetchAllConditional} from 'Util/EntityUtils';
import { observer } from 'mobx-react';
import { isOrCondition } from "../../../Util/GraphQLUtils";
import { AttributeCRUDOptions } from 'Models/CRUDOptions';

export type Comparators = 'contains' | 'endsWith' | 'equal' | 'greaterThan' | 'greaterThanOrEqual' | 'in' | 'notIn' | 'lessThan' | 'lessThanOrEqual' | 'like' | 'notEqual' | 'startsWith';

export interface IOrderByCondition<T> {
	path: string;
	descending?: boolean;
}

export type CaseComparison =
	'CURRENT_CULTURE' |
	'CURRENT_CULTURE_IGNORE_CASE' |
	'INVARIANT_CULTURE' |
	'INVARIANT_CULTURE_IGNORE_CASE' |
	'ORDINAL' |
	'ORDINAL_IGNORE_CASE';

export type CaseComparisonPascalCase =
	'CurrentCulture' |
	'CurrentCultureIgnoreCase' |
	'InvariantCulture' |
	'InvariantCultureIgnoreCase' |
	'Ordinal' |
	'OrdinalIgnoreCase';

interface BaseWhereCondition<T> {
	path: string;
	comparison: Comparators;
	value: any;
}

export interface IWhereCondition<T> extends BaseWhereCondition<T> {
	case?: CaseComparison;
}

export interface IWhereConditionApi<T> extends BaseWhereCondition<T> {
	case?: CaseComparisonPascalCase;
}

export interface IModelQueryVariables<T> {
	skip?: number; 
	take?: number; 
	args?: Array<IWhereCondition<T>>;
	orderBy: Array<IOrderByCondition<T>>;
	ids?: string[];
}

export interface IModelQueryProps<T extends Model, TData = any> {
	children: (result: QueryResult<TData, OperationVariables>) => JSX.Element | null;
	model: {new(json?: {}): T};
	conditions?: Array<IWhereCondition<T>> | Array<Array<IWhereCondition<T>>>;
	ids?: string[];
	orderBy?: IOrderByCondition<T>;
	customQuery?: DocumentNode;
	pagination: PaginationQueryOptions;
}

@observer
class ModelQuery<T extends Model,TData = any> extends React.Component<IModelQueryProps<T, TData>> {
	public render() {
		const modelName = getModelName(this.props.model);
		let fetchAllQuery;
		
		if (isOrCondition(this.props.conditions)) {
			fetchAllQuery = getFetchAllConditional(this.props.model);
		} else {
			fetchAllQuery = getFetchAllQuery(this.props.model);
		}

		return (
			<Query 
				fetchPolicy="network-only"
				notifyOnNetworkStatusChange={true}
				query={this.props.customQuery || fetchAllQuery} 
				variables={this.constructVariables()} >
				{this.props.children}
			</Query>
		);
	}

	private constructVariables() {
		const { conditions, ids, orderBy : orderByProp, pagination } = this.props;
		const { page, perPage } = pagination;

		let orderBy: IOrderByCondition<T> = {
			path: new this.props.model().getDisplayAttribute(),
			descending: false
		};

		if (orderByProp) {
			orderBy = orderByProp;
		}

		return {
			skip: page * perPage, 
			take: perPage, 
			args: conditions,
			orderBy: [orderBy],
			ids,
		};
	}
}

export default ModelQuery;