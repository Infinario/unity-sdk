//
//  Customer.h
//  InfinarioSDK
//
//  Created by Igi on 2/5/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import "Command.h"

@interface Customer : Command

- (instancetype)initWithIds:(NSDictionary *)ids andProjectId:(NSString *)projectId andWithProperties:(NSDictionary *)properties;

@end
